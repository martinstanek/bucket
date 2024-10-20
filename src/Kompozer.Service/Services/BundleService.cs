using Kompozer.Service.Model;
using Kompozer.Service.Serialization;
using System.Formats.Tar;
using System.IO.Compression;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Linq;
using Ardalis.GuardClauses;

namespace Kompozer.Service.Services;

public sealed class BundleService
{
    private readonly DockerService _dockerService;
    private readonly FileSystemService _fileSystemService;

    public BundleService(DockerService dockerService, FileSystemService fileSystemService)
    {
        _dockerService = dockerService;
        _fileSystemService = fileSystemService;
    }

    public bool TryFindBundleManifest(out BundleManifest? definition, out string definitionPath)
    {
        definition = default;
        definitionPath = string.Empty;

        var workDir = AppContext.BaseDirectory;
        var files = Directory.GetFiles(workDir).Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            if (TryParseBundleManifest(file, out var bundleDefinition) && bundleDefinition is not null)
            {
                definition = bundleDefinition;
                definitionPath = file;

                return true;
            }
        }

        return false;
    }

    public async Task CheckDockerAsync()
    {
        var version = await _dockerService.GetVersionAsync();
        
        Console.WriteLine(version);
    }

    public async Task CreateBundleAsync(BundleManifest bundleManifest, string manifestPath, string workDir)
    {
        Guard.Against.NullOrWhiteSpace(workDir);
        Guard.Against.NullOrWhiteSpace(manifestPath);

        if (!File.Exists(manifestPath))
        {
            throw new InvalidOperationException("Can't find the manifest file");
        }

        if (Directory.Exists(workDir))
        {
            throw new InvalidOperationException("Working directory does not exist.");
        }

        var bundleDirectory = Path.Combine(workDir, "_bundle");

        Directory.CreateDirectory(bundleDirectory);

        await ExportImagesAsync(bundleManifest, bundleDirectory);

        CopyContent(bundleManifest, bundleDirectory, manifestPath);

        await PackBundleAsync(bundleManifest, bundleDirectory);

        CleanUp(bundleDirectory);
    }

    private async Task ExportImagesAsync(BundleManifest bundleDefinition, string workDir)
    {
        if (!bundleDefinition.Configuration.FetchImages)
        {
            return;
        }

        Console.WriteLine("Pulling images ...");

        foreach (var image in bundleDefinition.Images)
        {
            Console.WriteLine(await _dockerService.PullImageAsync(image.FullName));
        }

        var exportDirectory = Path.Combine(workDir, "_export");

        Directory.CreateDirectory(exportDirectory);
        Console.WriteLine($"Exporting images into: {exportDirectory}");

        foreach (var image in bundleDefinition.Images)
        {
            var imageName = $"{image.Alias}.tar";
            var fullPath = Path.Combine(exportDirectory, imageName);

            await _dockerService.ExportImageAsync(image.FullName, fullPath);

            Console.WriteLine(imageName);
        }
    }

    private static async Task PackBundleAsync(BundleManifest bundleDefinition, string workDir)
    {
        Console.WriteLine("Packing bundle ...");

        var bundleName = $"./{bundleDefinition.Info.Name}.dap.tar.gz";
        
        await using var fs = new FileStream(bundleName, FileMode.CreateNew, FileAccess.Write);
        await using var gz = new GZipStream(fs, CompressionMode.Compress, leaveOpen: true);

        await TarFile.CreateFromDirectoryAsync(workDir, gz, includeBaseDirectory: false);
    }

    private static bool TryParseBundleManifest(string manifestPath, out BundleManifest? definition)
    {
        var content = File.ReadAllText(manifestPath);
        definition = default;

        try
        {
            definition = JsonSerializer.Deserialize(content, SourceGenerationContext.Default.BundleManifest);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void CopyContent(BundleManifest bundleManifest, string workDir, string manifestPath)
    {
        var stacksBundleDirectory = Path.Combine(workDir, "_stacks");

        Directory.CreateDirectory(stacksBundleDirectory);

        File.Copy(manifestPath, Path.Combine(workDir, "manifest.json"));

        foreach (var bundleDefinitionStack in bundleManifest.Stacks)
        {
            _fileSystemService.CopyDirectory(bundleDefinitionStack, Path.Combine(stacksBundleDirectory, bundleDefinitionStack));
        }
    }

    private static void CleanUp(string workDir)
    {
        Console.WriteLine("Cleaning ...");

        Directory.Delete(workDir, recursive: true);
    }
}