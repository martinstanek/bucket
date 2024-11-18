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
        using var context = new BucketWorkerTestContext();
        var bundleWorker = context.GetBucketWorker("--bundle", "./Bundle/manifest.json", "--workdir", context.Workdir, "--output", context.Workdir);
        var installWorker = context.GetBucketWorker("--install",  context.BundlePath, "--output", context.BundleFolderPath);
        var removeWorker = context.GetBucketWorker("--remove", context.BundleManifestPath);
        
        await bundleWorker.StartAsync(CancellationToken.None);
        await installWorker.StartAsync(CancellationToken.None);
        await removeWorker.StartAsync(CancellationToken.None);
    }
    
    private sealed class BucketWorkerTestContext : IDisposable
    {
        private readonly string _workDir = Path.Combine("./", Guid.NewGuid().ToString("N"));
        
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

        public void Dispose()
        {
            if (Directory.Exists(_workDir))
            {
                Directory.Delete(_workDir, recursive: true);
            }
        }

        internal string Workdir => _workDir;

        internal string BundlePath => Path.Combine(_workDir, "bucket-test-bundle.dap.tar.gz");
        
        internal string BundleFolderPath => Path.Combine(_workDir, "dap");
        
        internal string BundleManifestPath => Path.Combine(BundleFolderPath, "manifest.json");
    }
}