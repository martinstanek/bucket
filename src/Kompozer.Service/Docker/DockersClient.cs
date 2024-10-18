using System.Diagnostics;
using System.Threading.Tasks;

namespace Kompozer.Service.Docker;

public sealed class DockersClient
{
    public Task<string> GetVersionAsync()
    {
        var info = new ProcessStartInfo
        {
            FileName = "docker",
            Arguments = "--version",
            RedirectStandardOutput = true
        };

        var process = Process.Start(info);

        if (process is null)
        {
            return Task.FromResult("");
        }

        var line = string.Empty;

        while (!process.StandardOutput.EndOfStream)
        {
            line = process.StandardOutput.ReadLine();
        }

        return Task.FromResult(process.ExitCode + " > " + line);
    }
}