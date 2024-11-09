using System.Threading;
using System.Threading.Tasks;

namespace Bucket.Service.Services;

public interface IBundleService
{
    Task BundleAsync(string inputManifest, string outputPath, CancellationToken cancellationToken = default);
    Task InstallAsync(string bundlePath, CancellationToken cancellationToken = default);
    Task StopAsync(string manifestPath, CancellationToken cancellationToken = default);
    Task StartAsync(string manifestPath, CancellationToken cancellationToken = default);
}