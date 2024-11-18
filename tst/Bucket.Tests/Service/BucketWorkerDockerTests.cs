using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Bucket.Service;
using Bucket.Service.Extensions;
using Bucket.Service.Services;
using Moq;
using Xunit;

namespace Bucket.Tests.Service;

public sealed class BucketWorkerDockerTests
{
    [Fact]
    public async Task BundleInstallRemove_BundleIsCreatedInstalledRemoved()
    {
        var workDir = Path.Combine("./", Guid.NewGuid().ToString("N"));
        var bundlePath = Path.Combine(workDir, "bucket-test-bundle.dap.tar.gz");
        var bundleFolderPath = Path.Combine(workDir, "dap");
        var bundleManifestPath = Path.Combine(bundleFolderPath, "manifest.json");
        var context = new BucketWorkerTestContext();
        var bundleWorker = context.GetBucketWorker("--bundle", "./Bundle/manifest.json", "--workdir", workDir, "--output", workDir);
        var installWorker = context.GetBucketWorker("--install",  bundlePath, "--output", bundleFolderPath);
        var removeWorker = context.GetBucketWorker("--remove", bundleManifestPath);
        
        await bundleWorker.StartAsync(CancellationToken.None);
        await installWorker.StartAsync(CancellationToken.None);
        await removeWorker.StartAsync(CancellationToken.None);
        
        Directory.Delete(workDir, recursive: true);
    }
    
    private sealed class BucketWorkerTestContext
    {
        public BucketWorker GetBucketWorker(params string[] args)
        {
            var output = new Mock<IOutput>();
            var lideCycle = new Mock<IHostApplicationLifetime>();
            var services = new ServiceCollection()
                .AddBucket(output.Object, args)
                .RemoveAll<IHostApplicationLifetime>()
                .AddSingleton(lideCycle.Object);
            
            return services
                .BuildServiceProvider()
                .GetRequiredService<BucketWorker>();
        }
    }
}