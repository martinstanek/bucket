using System.Diagnostics;
using System.Threading.Tasks;

namespace Kompozer.Service.Docker;

public sealed class DockersClient
{
    public async Task<string> GetVersionAsync()
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
            return "";
        }

        var line = string.Empty;

        while (!process.StandardOutput.EndOfStream)
        {
            line = await process.StandardOutput.ReadLineAsync();
        }

        await process.WaitForExitAsync();

        return $"{process.ExitCode} > {line}";
    }
}