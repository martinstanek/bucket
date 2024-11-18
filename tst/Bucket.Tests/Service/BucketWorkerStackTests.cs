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
        var workDir = Path.Combine("./", Guid.NewGuid().ToString("N"));
        var bundlePath = Path.Combine(workDir, "bucket-test-bundle.dap.tar.gz");
        var bundleFolderPath = Path.Combine(workDir, "dap");
        var context = new BucketWorkerTestContext();
        var bundleWorker = context.GetBucketWorker("--bundle", "./Bundle/manifest.json", "--workdir", workDir, "--output", workDir);
        var installWorker = context.GetBucketWorker("--install",  bundlePath, "--output", bundleFolderPath);
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await bundleWorker.StartAsync(CancellationToken.None);
        await installWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.UpStackAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        
        Directory.Delete(workDir, recursive: true);
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
        var workDir = Path.Combine("./", Guid.NewGuid().ToString("N"));
        var bundlePath = Path.Combine(workDir, "bucket-test-bundle.dap.tar.gz");
        var bundleFolderPath = Path.Combine(workDir, "dap");
        var bundleManifestPath = Path.Combine(bundleFolderPath, "manifest.json");
        var context = new BucketWorkerTestContext();
        var bundleWorker = context.GetBucketWorker("--bundle", "./Bundle/manifest.json", "--workdir", workDir, "--output", workDir);
        var installWorker = context.GetBucketWorker("--install",  bundlePath, "--output", bundleFolderPath);
        var stopWorker = context.GetBucketWorker("--stop", bundleManifestPath);
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await bundleWorker.StartAsync(CancellationToken.None);
        await installWorker.StartAsync(CancellationToken.None);
        await stopWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.StopContainerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        
        Directory.Delete(workDir, recursive: true);
    }
    
    [Fact]
    public async Task Execute_Remove_RemovalExecuted()
    {
        var workDir = Path.Combine("./", Guid.NewGuid().ToString("N"));
        var bundlePath = Path.Combine(workDir, "bucket-test-bundle.dap.tar.gz");
        var bundleFolderPath = Path.Combine(workDir, "dap");
        var bundleManifestPath = Path.Combine(bundleFolderPath, "manifest.json");
        var context = new BucketWorkerTestContext();
        var bundleWorker = context.GetBucketWorker("--bundle", "./Bundle/manifest.json", "--workdir", workDir, "--output", workDir);
        var installWorker = context.GetBucketWorker("--install",  bundlePath, "--output", bundleFolderPath);
        var removeWorker = context.GetBucketWorker("-r", bundleManifestPath);
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        await bundleWorker.StartAsync(CancellationToken.None);
        await installWorker.StartAsync(CancellationToken.None);
        await removeWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.DownStackAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.RemoveImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        
        Directory.Delete(workDir, recursive: true);
    }
    
    [Fact]
    public async Task Execute_Start_StartExecuted()
    {
        var workDir = Path.Combine("./", Guid.NewGuid().ToString("N"));
        var bundlePath = Path.Combine(workDir, "bucket-test-bundle.dap.tar.gz");
        var bundleFolderPath = Path.Combine(workDir, "dap");
        var bundleManifestPath = Path.Combine(bundleFolderPath, "manifest.json");
        var context = new BucketWorkerTestContext();
        var bundleWorker = context.GetBucketWorker("--bundle", "./Bundle/manifest.json", "--workdir", workDir, "--output", workDir);
        var installWorker = context.GetBucketWorker("--install",  bundlePath, "--output", bundleFolderPath);
        var startWorker = context.GetBucketWorker("-s", bundleManifestPath);
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await bundleWorker.StartAsync(CancellationToken.None);
        await installWorker.StartAsync(CancellationToken.None);
        await startWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.UpStackAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        
        Directory.Delete(workDir, recursive: true);
    }
    
    private sealed class BucketWorkerTestContext
    {
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

        internal Mock<IHostApplicationLifetime> HostLifeTime { get; } = new();
        
        internal Mock<IDockerService> DockerService { get; } = new();
    }
}