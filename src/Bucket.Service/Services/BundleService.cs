using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Bucket.Service.Model;
using Bucket.Service.Serialization;
using Microsoft.Extensions.Logging;

namespace Bucket.Service.Services;

public sealed class BundleService : IBundleService
{
    private const string BundleExtension = ".dap.tar.gz";
    private const string ExportedImageExtension = ".tar";
    private const string ManifestFile = "manifest.json";
    private const string ComposeFile = "docker-compose.yml";
    private const string ExportFolder = "_export";
    private const string StacksFolder = "_stacks";
    private const string BundleFolder = "_bundle";
    
    private readonly IFileSystemService _fileSystemService;
    private readonly ICompressorService _compressorService;
    private readonly IDockerService _dockerService;
    private readonly ILogger<BundleService> _logger;

    public BundleService(
        IFileSystemService fileSystemService,
        ICompressorService compressorService,
        IDockerService dockerService,
        ILogger<BundleService> logger)
    {
        _fileSystemService = fileSystemService;
        _compressorService = compressorService;
        _dockerService = dockerService;
        _logger = logger;
    }

    public async Task BundleAsync(string manifestPath, string outputBundlePath, string workingDirectory, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(manifestPath);
        
        if (!await _dockerService.IsDockerRunningAsync())
        {
            return;
        }

        if (!TryParseBundleManifest(manifestPath, out var bundleDefinition) || bundleDefinition is null) 
        {
            Console.WriteLine("Failed to find manifest file");
            
            return;
        }
        
        var outputDir = GetWorkingDirectory(outputBundlePath);
        var workDir = GetWorkingDirectory(workingDirectory);
    
        Console.WriteLine("The manifest found and parsed:");
        Console.WriteLine($"{bundleDefinition.Info.Name} - {bundleDefinition.Info.Version}");

        await CreateBundleAsync(bundleDefinition, manifestPath, workDir, outputDir, cancellationToken);

        Console.WriteLine("Done");
    }

    public async Task InstallAsync(string bundlePath, string outputDirectory, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(bundlePath);
        Guard.Against.NullOrWhiteSpace(outputDirectory);

        if (!await _dockerService.IsDockerRunningAsync())
        {
            return;
        }
        
        if (!File.Exists(bundlePath))
        {
            Console.WriteLine($"The bundle '{bundlePath}' was not found");

            return;
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }
        
        await _compressorService.UnpackBundleAsync(bundlePath, outputDirectory, cancellationToken);

        var manifestPath = Path.Combine(outputDirectory, ManifestFile);

        if (TryParseBundleManifest(manifestPath, out var bundleManifest) && bundleManifest is not null)
        {
            await InstallBundleAsync(bundleManifest, outputDirectory, cancellationToken);
            
            return;
        }
        
        Console.WriteLine("Invalid bundle");
    }

    public Task RemoveAsync(string bundlePath, CancellationToken cancellationToken = default)
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

    private async Task CreateBundleAsync(BundleManifest bundleManifest, string manifestPath, string workingDirectory, string outputDirectory, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(workingDirectory);
        Guard.Against.NullOrWhiteSpace(manifestPath);
        Guard.Against.NullOrWhiteSpace(outputDirectory);

        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException(manifestPath);
        }

        if (!Directory.Exists(workingDirectory))
        {
            throw new DirectoryNotFoundException(workingDirectory);
        }
        
        if (!Directory.Exists(outputDirectory))
        {
            throw new DirectoryNotFoundException(outputDirectory);
        }

        var bundleDirectory = Path.Combine(workingDirectory, BundleFolder);

        Directory.CreateDirectory(bundleDirectory);
        
        _logger.LogInformation("Creating bundle");
        _logger.LogInformation($"Manifest: {manifestPath}");
        _logger.LogInformation($"Working directory: {workingDirectory}");
        _logger.LogInformation($"Outpit directory: {outputDirectory}");

        await ExportImagesAsync(bundleManifest, bundleDirectory, cancellationToken);

        CopyContent(bundleManifest, bundleDirectory, manifestPath);

        await _compressorService.PackBundleAsync(
            bundleManifest, 
            bundleDirectory, 
            outputDirectory,  
            BundleExtension, 
            cancellationToken);

        _logger.LogInformation("Deleting working directory");
        
        Directory.Delete(bundleDirectory, recursive: true);
    }

    private async Task ExportImagesAsync(BundleManifest bundleDefinition, string workDir, CancellationToken cancellationToken)
    {
        if (!bundleDefinition.Configuration.FetchImages)
        {
            return;
        }

        await PullImagesAsync(bundleDefinition);

        var exportDirectory = Path.Combine(workDir, ExportFolder);

        Directory.CreateDirectory(exportDirectory);
        Console.WriteLine($"Exporting images into: {exportDirectory}");

        await Parallel.ForEachAsync(bundleDefinition.Images, cancellationToken, async (image, _) =>
        {
            var imageName = $"{image.Alias}{ExportedImageExtension}";
            var fullPath = Path.Combine(exportDirectory, imageName);

            await _dockerService.SaveImageAsync(image.FullName, fullPath);

            Console.WriteLine(imageName);
        });
    }

    private async Task InstallBundleAsync(BundleManifest bundleManifest, string directory, CancellationToken cancellationToken)
    {
        if (bundleManifest.Configuration.FetchImages)
        {
            await ImportImagesAsync(bundleManifest, directory, cancellationToken);
        }
        else
        {
            await PullImagesAsync(bundleManifest);            
        }

        await SpinUpStacksAsync(bundleManifest, directory);
    }

    private async Task ImportImagesAsync(BundleManifest bundleManifest, string directory, CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(bundleManifest.Images, cancellationToken, async (image, token) =>
        {
            var path = Path.Combine(directory, ExportFolder, $"{image.Alias}{ExportedImageExtension}");

            if (File.Exists(path))
            {
                Console.WriteLine($"Importing: {path}");

                await _dockerService.LoadImageAsync(path);
            }
        });
    }

    private async Task SpinUpStacksAsync(BundleManifest bundleManifest, string directory)
    {
        foreach (var stack in bundleManifest.Stacks)
        {
            var stackFolder = stack.Trim('.').Trim('/');
            var path = Path.Combine(directory, StacksFolder, stackFolder, ComposeFile);

            if (File.Exists(path))
            {
                await _dockerService.UpStackAsync(path);
            }
        }
    }

    private async Task PullImagesAsync(BundleManifest bundleDefinition)
    {
        foreach (var image in bundleDefinition.Images)
        {
            Console.WriteLine(await _dockerService.PullImageAsync(image.FullName));
        }
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
        _logger.LogInformation("Copying content");
        
        var stacksBundleDirectory = Path.Combine(workDir, StacksFolder);

        Directory.CreateDirectory(stacksBundleDirectory);

        File.Copy(manifestPath, Path.Combine(workDir, ManifestFile));

        foreach (var bundleDefinitionStack in bundleManifest.Stacks)
        {
            _logger.LogInformation($"Copying stack: {bundleDefinitionStack}");
            
            _fileSystemService.CopyDirectory(bundleDefinitionStack, Path.Combine(stacksBundleDirectory, bundleDefinitionStack));
        }
    }

    private static string GetWorkingDirectory(string workingDirectory)
    {
        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            return AppContext.BaseDirectory;
        }

        if (!Directory.Exists(workingDirectory))
        {
            Directory.CreateDirectory(workingDirectory);
        }

        return workingDirectory;
    }
}