using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Bucket.Service;
using Bucket.Service.Services;
using Bucket.Service.Extensions;
using Moq;
using Xunit;

namespace Bucket.Tests.Service;

public sealed class BucketWorkerStackTests
{
    [Fact]
    public async Task Execute_Install_InstallationExecuted()
    {
        using var context = new BucketWorkerTestContext();
        var bundleWorker = context.GetBucketWorker("--bundle", "./Bundle/manifest.json", "--workdir", context.Workdir, "--output", context.Workdir);
        var installWorker = context.GetBucketWorker("--install",  context.BundlePath, "--output", context.BundleFolderPath);

        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await bundleWorker.StartAsync(CancellationToken.None);
        await installWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.UpStackAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Execute_Bundle_BundlingExecuted()
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker("-b", "./Bundle/manifest.json", "-v");
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        await worker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.DockerService.Verify(v => v.PullImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.SaveImageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Execute_Stop_StopExecuted()
    {
        using var context = new BucketWorkerTestContext();
        var bundleWorker = context.GetBucketWorker("--bundle", "./Bundle/manifest.json", "--workdir", context.Workdir, "--output", context.Workdir);
        var installWorker = context.GetBucketWorker("--install",  context.BundlePath, "--output", context.BundleFolderPath);
        var stopWorker = context.GetBucketWorker("--stop", context.BundleManifestPath);
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await bundleWorker.StartAsync(CancellationToken.None);
        await installWorker.StartAsync(CancellationToken.None);
        await stopWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.StopContainerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Execute_Remove_RemovalExecuted()
    {
        using var context = new BucketWorkerTestContext();
        var bundleWorker = context.GetBucketWorker("--bundle", "./Bundle/manifest.json", "--workdir", context.Workdir, "--output", context.Workdir);
        var installWorker = context.GetBucketWorker("--install",  context.BundlePath, "--output", context.BundleFolderPath);
        var removeWorker = context.GetBucketWorker("-r", context.BundleManifestPath);
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        await bundleWorker.StartAsync(CancellationToken.None);
        await installWorker.StartAsync(CancellationToken.None);
        await removeWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.DownStackAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.RemoveImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Execute_Start_StartExecuted()
    {
        var context = new BucketWorkerTestContext();
        var bundleWorker = context.GetBucketWorker("--bundle", "./Bundle/manifest.json", "--workdir", context.Workdir, "--output", context.Workdir);
        var installWorker = context.GetBucketWorker("--install",  context.BundlePath, "--output", context.BundleFolderPath);
        var startWorker = context.GetBucketWorker("-s", context.BundleManifestPath);
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await bundleWorker.StartAsync(CancellationToken.None);
        await installWorker.StartAsync(CancellationToken.None);
        await startWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.UpStackAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    private sealed class BucketWorkerTestContext : IDisposable
    {
        private readonly string _workDir = Path.Combine("./", Guid.NewGuid().ToString("N"));
        
        public BucketWorker GetBucketWorker(params string[] args)
        {
            var services = new ServiceCollection()
                .AddBucket(new Mock<IOutput>().Object, args)
                .RemoveAll<IHostApplicationLifetime>()
                .RemoveAll<IDockerService>()
                .AddSingleton(HostLifeTime.Object)
                .AddSingleton(DockerService.Object);
            
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

        internal Mock<IHostApplicationLifetime> HostLifeTime { get; } = new();
        
        internal Mock<IDockerService> DockerService { get; } = new();
        
        internal string Workdir => _workDir;

        internal string BundlePath => Path.Combine(_workDir, "bucket-test-bundle.dap.tar.gz");
        
        internal string BundleFolderPath => Path.Combine(_workDir, "dap");
        
        internal string BundleManifestPath => Path.Combine(BundleFolderPath, "manifest.json");
    }
}