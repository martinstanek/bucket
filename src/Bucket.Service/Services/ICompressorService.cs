using System.Threading;
using System.Threading.Tasks;
using Bucket.Service.Model;

namespace Bucket.Service.Services;

public interface ICompressorService
{
    Task<string> PackBundleAsync(
        BundleManifest bundleDefinition,
        string bundleDirectory,
        string outputDirectory,
        string extension,
        CancellationToken cancellationToken);

    Task UnpackBundleAsync(
        string bundlePath,
        string outputDirectory,
        CancellationToken cancellationToken);
}