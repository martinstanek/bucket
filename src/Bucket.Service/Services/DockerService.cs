using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;

namespace Bucket.Service.Services;

public sealed class DockerService : IDockerService
{
    private const int CancelCheckAfterSeconds = 5;
    
    private readonly ILogger<DockerService> _logger;

    public DockerService(ILogger<DockerService> logger)
    {
        _logger = logger;
    }
    
    public async Task<bool> IsDockerRunningAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking Docker daemon");

        var limitTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(CancelCheckAfterSeconds));
        var mergedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, limitTokenSource.Token);
        
        try
        {
            var stats = await GetDockerProcessesAsync(mergedTokenSource.Token);

            return !string.IsNullOrWhiteSpace(stats);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, e.Message);
            
            return false;
        }
    }

    public Task<string> GetVersionAsync(CancellationToken cancellationToken)
    {
        return RunDockerProcessAsync("--version", cancellationToken);
    }
    
    public Task<string> GetDockerProcessesAsync(CancellationToken cancellationToken)
    {
        return RunDockerProcessAsync("ps", cancellationToken);
    }

    public Task<string> PullImageAsync(string fullImageName, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);
        
        _logger.LogInformation($"Pulling image: {fullImageName}");

        return RunDockerProcessAsync($"pull {fullImageName}", cancellationToken);
    }

    public async Task ExportImageAsync(string fullImageName, string outputFile, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);
        Guard.Against.NullOrWhiteSpace(outputFile);
        
        _logger.LogInformation($"Creating container image: {fullImageName}");

        var id = await RunDockerProcessAsync($"create {fullImageName}", cancellationToken);
        
        _logger.LogInformation($"Exporting container id: {id}");
        
        await RunDockerProcessAsync($"export {id} -o {outputFile}", cancellationToken);
        
        _logger.LogInformation($"Removing container id: {id}");
        
        await RunDockerProcessAsync($"container rm {id}", cancellationToken);
    }
    
    public async Task SaveImageAsync(string fullImageName, string outputFile, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);
        Guard.Against.NullOrWhiteSpace(outputFile);
        
        _logger.LogInformation($"Saving image: {fullImageName}");

        await RunDockerProcessAsync($"save -o {outputFile} {fullImageName}", cancellationToken);
    }

    public async Task ImportImageAsync(string fullImageName, string inputFile, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);
        Guard.Against.NullOrWhiteSpace(inputFile);

        if (!File.Exists(inputFile))
        {
            _logger.LogWarning($"Input file not found: {inputFile}");
            
            return;
        }

        _logger.LogInformation($"Importing image: {fullImageName}");
        
        var id = await RunDockerProcessAsync($"image import {inputFile}", cancellationToken);
        
        _logger.LogInformation($"Re-tagging image id: {id}");
        
        await RunDockerProcessAsync($"tag {id} {fullImageName}", cancellationToken);
    }

    public async Task RemoveContainerAsync(string fullImageName, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);

        _logger.LogInformation($"Removing container based on the image: {fullImageName}");
        
        var id = await RunDockerProcessAsync($"ps -a -q --filter ancestor=\"{fullImageName}\"", cancellationToken);

        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning($"Can't find container based on the image: {fullImageName}");
            
            return;
        }
        
        _logger.LogInformation($"Removing container id: {id}");
        
        await RunDockerProcessAsync($"container rm {id}", cancellationToken);
    }

    public async Task StopContainerAsync(string fullImageName, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);

        var id = await RunDockerProcessAsync($"ps -a -q --filter ancestor=\"{fullImageName}\"", cancellationToken);
        
        if (string.IsNullOrWhiteSpace(id))
        {
            _logger.LogWarning($"Can't find container based on the image: {fullImageName}");
            
            return;
        }
        
        await RunDockerProcessAsync($"container stop {id}", cancellationToken);
    }

    public Task RemoveImageAsync(string fullImageName, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);
        
        _logger.LogInformation($"Removing image: {fullImageName}");

        return RunDockerProcessAsync($"image rm {fullImageName}", cancellationToken);
    }

    public async Task LoadImageAsync(string inputFile, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(inputFile);

        if (!File.Exists(inputFile))
        {
            _logger.LogWarning($"Input file not found: {inputFile}");
            
            return;
        }

        await RunDockerProcessAsync($"load -i {inputFile}", cancellationToken);
    }
    
    public Task UpStackAsync(string composeFilePath, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(composeFilePath);
        
        if (!File.Exists(composeFilePath))
        {
            _logger.LogWarning($"Compose file not found: {composeFilePath}");
            
            return Task.CompletedTask;
        }

        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? RunProcessAsync("docker-compose", $"-f \"{composeFilePath}\" up -d", cancellationToken)
            : RunDockerProcessAsync($"compose -f \"{composeFilePath}\" up -d --build", cancellationToken);
    }

    public Task DownStackAsync(string composeFilePath, CancellationToken cancellationToken)
    {
        Guard.Against.NullOrWhiteSpace(composeFilePath);
        
        if (!File.Exists(composeFilePath))
        {
            _logger.LogWarning($"Compose file not found: {composeFilePath}");
            
            return Task.CompletedTask;
        }
        
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? RunProcessAsync("docker-compose", $"-f \"{composeFilePath}\" down", cancellationToken)
            : RunDockerProcessAsync($"compose -f \"{composeFilePath}\" down", cancellationToken);
    }

    private Task<string> RunDockerProcessAsync(string arguments, CancellationToken cancellationToken)
    {
        return RunProcessAsync("docker", arguments, cancellationToken);
    }

    private async Task<string> RunProcessAsync(string name, string arguments, CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo
        {
            FileName = name,
            Arguments = arguments,
            RedirectStandardOutput = true
        };

        _logger.LogInformation($"Starting process: {name} {arguments}");
        
        var process = Process.Start(info);

        if (process is null)
        {
            throw new InvalidOperationException("Process can not be executed");
        }

        var output = string.Empty;

        while (!process.StandardOutput.EndOfStream)
        {
            output = await process.StandardOutput.ReadLineAsync(cancellationToken);
        }

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("Process did not exited with the expected response code");
        }

        return output ?? string.Empty;
    }
}