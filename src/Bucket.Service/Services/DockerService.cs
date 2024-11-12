using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Ardalis.GuardClauses;

namespace Bucket.Service.Services;

public sealed class DockerService : IDockerService
{
    public async Task<bool> IsDockerRunningAsync()
    {
        try
        {
            var version = await GetVersionAsync();

            return !string.IsNullOrWhiteSpace(version);
        }
        catch
        {
            return false;
        }
    }

    public Task<string> GetVersionAsync()
    {
        return RunDockerProcessAsync("--version");
    }

    public Task<string> PullImageAsync(string fullImageName)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);

        return RunDockerProcessAsync($"pull {fullImageName}");
    }

    public async Task ExportImageAsync(string fullImageName, string outputFile)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);
        Guard.Against.NullOrWhiteSpace(outputFile);

        var id = await RunDockerProcessAsync($"create {fullImageName}");
        await RunDockerProcessAsync($"export {id} -o {outputFile}");
        await RunDockerProcessAsync($"container rm {id}");
    }
    
    public async Task SaveImageAsync(string fullImageName, string outputFile)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);
        Guard.Against.NullOrWhiteSpace(outputFile);

        await RunDockerProcessAsync($"save -o {outputFile} {fullImageName}");
    }

    public async Task ImportImageAsync(string fullImageName, string inputFile)
    {
        Guard.Against.NullOrWhiteSpace(fullImageName);
        Guard.Against.NullOrWhiteSpace(inputFile);

        if (!File.Exists(inputFile))
        {
            return;
        }

        var id = await RunDockerProcessAsync($"image import {inputFile}");
        await RunDockerProcessAsync($"tag {id} {fullImageName}");
    }
    
    public async Task LoadImageAsync(string inputFile)
    {
        Guard.Against.NullOrWhiteSpace(inputFile);

        if (!File.Exists(inputFile))
        {
            return;
        }

        await RunDockerProcessAsync($"load -i {inputFile}");
    }
    
    public async Task UpStackAsync(string composeFilePath)
    {
        Guard.Against.NullOrWhiteSpace(composeFilePath);
        
        if (!File.Exists(composeFilePath))
        {
            return;
        }
        
        await RunDockerProcessAsync($"compose -f \"{composeFilePath}\" up -d --build");
    }

    private static Task<string> RunDockerProcessAsync(string arguments)
    {
        return RunProcessAsync("docker", arguments);
    }

    private static async Task<string> RunProcessAsync(string name, string arguments)
    {
        var info = new ProcessStartInfo
        {
            FileName = name,
            Arguments = arguments,
            RedirectStandardOutput = true
        };

        var process = Process.Start(info);

        if (process is null)
        {
            throw new InvalidOperationException("Process can not be executed");
        }

        var output = string.Empty;

        while (!process.StandardOutput.EndOfStream)
        {
            output = await process.StandardOutput.ReadLineAsync();
        }

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException("Process did not exited with the expected response code");
        }

        return output ?? string.Empty;
    }
}