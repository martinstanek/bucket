using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Kompozer.Service.Docker;
using Kompozer.Service.Model;
using Kompozer.Service.Serialization;
using System.Formats.Tar;
using System.IO.Compression;

namespace Kompozer.Service;

public sealed class HelloService : BackgroundService
{
    private readonly DockersClient _dockerClient;

    public HelloService(DockersClient dockersClient)
    {
        _dockerClient = dockersClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!TryFindBundleDefinition(out var bundleDefinition) || bundleDefinition is null)
        {
            Console.WriteLine("No bundle definition has been found ...");
            return;
        }

        Console.WriteLine(bundleDefinition.Info);
        
        var imagesDir = await PrepareBundleAsync(bundleDefinition);

        Console.WriteLine("Creating bundle ...");

        await PackBundleAsync(bundleDefinition, imagesDir);

        Console.WriteLine("Done");
    }

    private async Task<string> PrepareBundleAsync(BundleDefinition bundleDefinition)
    {
        Console.WriteLine(await _dockerClient.GetVersionAsync());
        Console.WriteLine("Pulling images ...");

        foreach (var image in bundleDefinition.Images)
        {
            Console.WriteLine(await _dockerClient.PullImageAsync(image.FullName));
        }

        var exportDirectory = Path.Combine(AppContext.BaseDirectory, "_images");

        Directory.CreateDirectory(exportDirectory);
        Console.WriteLine($"Exporting images into: {exportDirectory}");

        foreach (var image in bundleDefinition.Images)
        {
            Console.WriteLine(await _dockerClient.ExportImageAsync(image.FullName, Path.Combine(exportDirectory, $"{image.Alias}.tar")));
        }

        return exportDirectory;
    }

    private static async Task PackBundleAsync(BundleDefinition bundleDefinition, string imagesDirectory)
    {
        var bundleDirectory = Path.Combine(AppContext.BaseDirectory, "_bundle");

        Directory.CreateDirectory(bundleDirectory);
        
        CopyDirectory(imagesDirectory, bundleDirectory);

        foreach (var bundleDefinitionStack in bundleDefinition.Stacks)
        {
            CopyDirectory(bundleDefinitionStack, Path.Combine(bundleDirectory, bundleDefinitionStack));
        }
        
        var bundleName = $"./{bundleDefinition.Info.Name}.dap.tar.gz";

        await using var fs = new FileStream(bundleName, FileMode.CreateNew, FileAccess.Write);
        await using var gz = new GZipStream(fs, CompressionMode.Compress, leaveOpen: true);

        await TarFile.CreateFromDirectoryAsync(bundleDirectory, bundleName, includeBaseDirectory: false);
    }

    private static bool TryFindBundleDefinition(out BundleDefinition? definition)
    {
        definition = default;

        var workDir = AppContext.BaseDirectory;
        var files = Directory.GetFiles(workDir).Where(f => f.EndsWith("json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            if (TryParseBundleDefinition(file, out var bundleDefinition) && bundleDefinition is not null)
            {
                definition = bundleDefinition;
                return true;
            }
        }

        return false;
    }

    private static bool TryParseBundleDefinition(string path, out BundleDefinition? definition)
    {
        var content = File.ReadAllText(path);
        definition = default;

        try
        {
            definition = JsonSerializer.Deserialize(content, SourceGenerationContext.Default.BundleDefinition);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
        }
        
        var dirs = dir.GetDirectories();

        Directory.CreateDirectory(destinationDir);

        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath);
        }

        if (!recursive)
        {
            return;
        }
        
        foreach (var subDir in dirs)
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);

            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }
}