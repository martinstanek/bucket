using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Bucket.Service.Model;
using Ardalis.GuardClauses;

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
    private readonly IOutput _output;
    private readonly ILogger<BundleService> _logger;

    public BundleService(
        IFileSystemService fileSystemService,
        ICompressorService compressorService,
        IDockerService dockerService,
        IOutput output,
        ILogger<BundleService> logger)
    {
        _fileSystemService = fileSystemService;
        _compressorService = compressorService;
        _dockerService = dockerService;
        _output = output;
        _logger = logger;
    }

    public async Task BundleAsync(string manifestPath, string outputBundlePath, string workingDirectory, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(manifestPath);

        if (!await _dockerService.IsDockerRunningAsync(cancellationToken))
        {
            return;
        }

        if (!BundleManifest.TryParseFromPath(manifestPath, out var bundleDefinition) || bundleDefinition is null)
        {
            _output.WriteLine("Failed to find manifest file");

            return;
        }

        var outputDir = GetWorkingDirectory(outputBundlePath);
        var workDir = GetWorkingDirectory(workingDirectory);

        _output.WriteLine("The manifest found and parsed:");
        _output.WriteLine($"{bundleDefinition.Info.Name} - {bundleDefinition.Info.Version}");

        _logger.LogInformation($"Manifest: {manifestPath}");
        _logger.LogInformation($"Output: {outputDir}");
        _logger.LogInformation($"Workdir: {workDir}");
        _logger.LogInformation($"Images: {bundleDefinition.Images.Count}");
        _logger.LogInformation($"Stacks: {bundleDefinition.Stacks.Count}");

        await CreateBundleAsync(bundleDefinition, manifestPath, workDir, outputDir, cancellationToken);

        _output.WriteLine("Done");
    }

    public async Task InstallAsync(string bundlePath, string outputDirectory, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(bundlePath);
        Guard.Against.NullOrWhiteSpace(outputDirectory);

        if (!await _dockerService.IsDockerRunningAsync(cancellationToken))
        {
            return;
        }

        if (!File.Exists(bundlePath))
        {
            _output.WriteLine($"The bundle '{bundlePath}' was not found");

            return;
        }

        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        await _compressorService.UnpackBundleAsync(bundlePath, outputDirectory, cancellationToken);

        var manifestPath = Path.Combine(outputDirectory, ManifestFile);

        if (BundleManifest.TryParseFromPath(manifestPath, out var bundleManifest) && bundleManifest is not null)
        {
            await InstallBundleAsync(bundleManifest, outputDirectory, cancellationToken);

            return;
        }

        _output.WriteLine("Invalid bundle");
    }

    public async Task RemoveAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(manifestPath);

        if (!await _dockerService.IsDockerRunningAsync(cancellationToken))
        {
            return;
        }

        if (!File.Exists(manifestPath))
        {
            _output.WriteLine($"The manifest '{manifestPath}' was not found");

            return;
        }

        if (BundleManifest.TryParseFromPath(manifestPath, out var bundleManifest) && bundleManifest is not null)
        {
            var directory = Path.GetDirectoryName(manifestPath) ?? string.Empty;

            await DownStacksAsync(bundleManifest, directory, cancellationToken);
            await RemoveStackArtifactsAsync(bundleManifest, cancellationToken);

            Directory.Delete(directory, recursive: true);

            return;
        }

        _output.WriteLine("Invalid bundle");
    }

    public async Task StopAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(manifestPath);

        if (!await _dockerService.IsDockerRunningAsync(cancellationToken))
        {
            return;
        }

        if (!File.Exists(manifestPath))
        {
            _output.WriteLine($"The manifest '{manifestPath}' was not found");

            return;
        }

        if (BundleManifest.TryParseFromPath(manifestPath, out var bundleManifest) && bundleManifest is not null)
        {
            await StopStacksAsync(bundleManifest, cancellationToken);

            return;
        }

        _output.WriteLine("Invalid bundle");
    }

    public async Task StartAsync(string manifestPath, CancellationToken cancellationToken = default)
    {
        Guard.Against.NullOrWhiteSpace(manifestPath);

        if (!await _dockerService.IsDockerRunningAsync(cancellationToken))
        {
            return;
        }

        if (!File.Exists(manifestPath))
        {
            _output.WriteLine($"The manifest '{manifestPath}' was not found");

            return;
        }

        if (BundleManifest.TryParseFromPath(manifestPath, out var bundleManifest) && bundleManifest is not null)
        {
            var directory = Path.GetDirectoryName(manifestPath) ?? string.Empty;

            await UpStacksAsync(bundleManifest, directory, cancellationToken);

            return;
        }

        _output.WriteLine("Invalid bundle");
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

        CopyContent(bundleManifest, bundleDirectory, manifestPath, cancellationToken);

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

        await PullImagesAsync(bundleDefinition, cancellationToken);

        var exportDirectory = Path.Combine(workDir, ExportFolder);

        Directory.CreateDirectory(exportDirectory);
        _output.WriteLine($"Exporting images into: {exportDirectory}");

        await Parallel.ForEachAsync(bundleDefinition.Images, cancellationToken, async (image, _) =>
        {
            var imageName = $"{image.Alias}{ExportedImageExtension}";
            var fullPath = Path.Combine(exportDirectory, imageName);

            await _dockerService.SaveImageAsync(image.FullName, fullPath, cancellationToken);

            _output.WriteLine(imageName);
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
            await PullImagesAsync(bundleManifest, cancellationToken);
        }

        await SpinUpStacksAsync(bundleManifest, directory, cancellationToken);
    }

    private async Task ImportImagesAsync(BundleManifest bundleManifest, string directory, CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(bundleManifest.Images, cancellationToken, async (image, token) =>
        {
            var path = Path.Combine(directory, ExportFolder, $"{image.Alias}{ExportedImageExtension}");

            if (File.Exists(path))
            {
                _output.WriteLine($"Importing: {path}");

                await _dockerService.LoadImageAsync(path, token);
            }
        });
    }

    private async Task SpinUpStacksAsync(BundleManifest bundleManifest, string directory, CancellationToken cancellationToken)
    {
        foreach (var stack in bundleManifest.Stacks)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var stackFolder = stack.Trim('.').Trim('/');
            var path = Path.Combine(directory, StacksFolder, stackFolder, ComposeFile);

            if (File.Exists(path))
            {
                await _dockerService.UpStackAsync(path, cancellationToken);
            }
        }
    }

    private async Task DownStacksAsync(BundleManifest bundleManifest, string directory, CancellationToken cancellationToken)
    {
        foreach (var stack in bundleManifest.Stacks)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var stackFolder = stack.Trim('.').Trim('/');
            var path = Path.Combine(directory, StacksFolder, stackFolder, ComposeFile);

            if (File.Exists(path))
            {
                await _dockerService.DownStackAsync(path, cancellationToken);
            }
        }
    }

    private async Task StopStacksAsync(BundleManifest bundleManifest, CancellationToken cancellationToken)
    {
        foreach (var image in bundleManifest.Images)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await _dockerService.StopContainerAsync(image.FullName, cancellationToken);
        }
    }

    private async Task UpStacksAsync(BundleManifest bundleManifest, string directory, CancellationToken cancellationToken)
    {
        foreach (var stack in bundleManifest.Stacks)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var stackFolder = stack.Trim('.').Trim('/');
            var path = Path.Combine(directory, StacksFolder, stackFolder, ComposeFile);

            if (File.Exists(path))
            {
                await _dockerService.UpStackAsync(path, cancellationToken);
            }
        }
    }

    private async Task RemoveStackArtifactsAsync(BundleManifest bundleManifest, CancellationToken cancellationToken)
    {
        foreach (var image in bundleManifest.Images)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await _dockerService.RemoveImageAsync(image.FullName, cancellationToken);
        }
    }

    private async Task PullImagesAsync(BundleManifest bundleDefinition, CancellationToken cancellationToken)
    {
        foreach (var image in bundleDefinition.Images)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            _output.WriteLine(await _dockerService.PullImageAsync(image.FullName, cancellationToken));
        }
    }

    private void CopyContent(BundleManifest bundleManifest, string workDir, string manifestPath, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Copying content");

        var stacksBundleDirectory = Path.Combine(workDir, StacksFolder);

        Directory.CreateDirectory(stacksBundleDirectory);

        File.Copy(manifestPath, Path.Combine(workDir, ManifestFile));

        foreach (var bundleDefinitionStack in bundleManifest.Stacks)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var manifestDirectory = Path.GetDirectoryName(manifestPath) ?? string.Empty;
            var stackDirectory = Path.Combine(manifestDirectory, bundleDefinitionStack);
            var stackOutputDirectory = Path.Combine(stacksBundleDirectory, bundleDefinitionStack);

            if (!Directory.Exists(stackDirectory))
            {
                _logger.LogWarning($"Stack directory not found: {stackDirectory}");

                continue;
            }

            _logger.LogInformation($"Copying stack folder: {stackDirectory} into {stackOutputDirectory}");

            _fileSystemService.CopyDirectory(stackDirectory, stackOutputDirectory);
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