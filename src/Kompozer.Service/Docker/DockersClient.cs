using System.Diagnostics;
using System.Threading.Tasks;

namespace Kompozer.Service.Docker;

public sealed class DockersClient
{
    public Task<string> GetVersionAsync()
    {
        var response = Process.Start("docker", "--version");

        return Task.FromResult("");
    }
}