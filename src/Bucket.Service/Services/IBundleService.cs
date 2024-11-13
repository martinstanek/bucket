using System.Threading;
using System.Threading.Tasks;

namespace Bucket.Service.Services;

public interface IBundleService
{
    Task BundleAsync(string manifestPath, string outputBundlePath, string workingDirectory, CancellationToken cancellationToken = default);
    
    Task InstallAsync(string bundlePath, string outputDirectory, CancellationToken cancellationToken = default);
    
    Task UninstallAsync(string bundlePath, CancellationToken cancellationToken = default);
    
    Task StopAsync(string manifestPath, CancellationToken cancellationToken = default);
    
    Task StartAsync(string manifestPath, CancellationToken cancellationToken = default);
}