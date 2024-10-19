using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Kompozer.Service.Docker;

public sealed class DockersClient
{
    public Task<string> GetVersionAsync()
    {
        return RunDockerProcessAsync("--version");
    }

    public Task<string> PullImageAsync(string fullImageName)
    {
        return RunDockerProcessAsync($"pull {fullImageName}");
    }

    public async Task<string> ExportImageAsync(string fullImageName, string outputFile)
    {
        var id = await RunDockerProcessAsync($"create {fullImageName}");

        return await RunDockerProcessAsync($"export {id} -o {outputFile}");
    }

    private Task<string> RunDockerProcessAsync(string arguments)
    {
        return RunProcessAsync("docker", arguments);
    }

    private async Task<string> RunProcessAsync(string name, string arguments)
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