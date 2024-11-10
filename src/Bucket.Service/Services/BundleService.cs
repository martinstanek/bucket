﻿using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Formats.Tar;
using System.IO.Compression;
using System.Threading.Tasks;
using Bucket.Service.Model;
using Bucket.Service.Serialization;
using Ardalis.GuardClauses;

namespace Bucket.Service.Services;

public sealed class BundleService : IBundleService
{
    private readonly IDockerService _dockerService;
    private readonly IFileSystemService _fileSystemService;

    public BundleService(IDockerService dockerService, IFileSystemService fileSystemService)
    {
        _dockerService = dockerService;
        _fileSystemService = fileSystemService;
    }

    public async Task BundleAsync(string manifestPath, string outputBundlePath, CancellationToken cancellationToken = default)
    {
        if (!await IsDockerRunningAsync())
        {
            return;
        }

        var searchResult = TryFindBundleManifest(manifestPath, cancellationToken);

        if (!searchResult.Found)
        {
            Console.WriteLine($"Failed to find manifest file");

            return;
        }

        Console.WriteLine("The manifest found and parsed:");
        Console.WriteLine($"{searchResult.Definition.Info.Name} - {searchResult.Definition.Info.Version}");

        await CreateBundleAsync(searchResult.Definition, manifestPath, AppContext.BaseDirectory);

        Console.WriteLine("Done");
    }

    public async Task InstallAsync(string bundlePath, string outputDirectory, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(bundlePath);
        Guard.Against.NullOrWhiteSpace(outputDirectory);

        if (!File.Exists(bundlePath))
        {
            Console.WriteLine($"The bundle '{bundlePath}' was not found");

            return;
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        if (!await IsDockerRunningAsync())
        {
            return;
        }
        
        await TarFile.ExtractToDirectoryAsync(bundlePath, outputDirectory, overwriteFiles: true, cancellationToken);
    }

    public Task UninstallAsync(string bundlePath, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("uninstalling ...");
        
        return Task.CompletedTask;
    }

    public Task StopAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("stopping ...");
        
        return Task.CompletedTask;
    }

    public Task StartAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        Console.WriteLine("starting ...");
        
        return Task.CompletedTask;
    }

    private async Task<bool> IsDockerRunningAsync()
    {
        try
        {
            var version = await _dockerService.GetVersionAsync();
        
            Console.WriteLine(version);

            return true;
        }
        catch
        {
            Console.WriteLine("Docker is not running.");
        }

        return false;
    }

    private async Task CreateBundleAsync(BundleManifest bundleManifest, string manifestPath, string workDir)
    {
        Guard.Against.NullOrWhiteSpace(workDir);
        Guard.Against.NullOrWhiteSpace(manifestPath);

        if (!File.Exists(manifestPath))
        {
            throw new InvalidOperationException("Can't find the manifest file");
        }

        if (!Directory.Exists(workDir))
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

    private static (bool Found, BundleManifest Definition, string Path) TryFindBundleManifest(string path, CancellationToken cancellationToken)
    {
        var result = (found: false, definition: BundleManifest.Empty, path: string.Empty);
        
        if (!string.IsNullOrWhiteSpace(path) && !File.Exists(path))
        {
            return result;
        }
        
        var workDir = AppContext.BaseDirectory;
        var files = Directory.GetFiles(workDir).Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            if (TryParseBundleManifest(file, out var bundleDefinition) && bundleDefinition is not null)
            {
                result.definition = bundleDefinition;
                result.path = file;
                result.found = true;
                
                return result;
            }
        }

        return result;
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