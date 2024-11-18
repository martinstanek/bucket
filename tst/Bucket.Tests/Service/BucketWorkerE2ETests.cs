using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Bucket.Service;
using Bucket.Service.Services;
using Bucket.Service.Extensions;
using Moq;
using Xunit;

namespace Bucket.Tests.Service;

public sealed class BucketWorkerE2ETests
{
    [Fact]
    public async Task Execute_Install_InstallationExecuted()
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker("-i", "./Data/bundle.dap.tar.gz", "-o", "./temp", "--verbose");
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        await worker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.DockerService.Verify(v => v.UpStackAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Execute_Bundle_BundlingExecuted()
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker("-b", "./Data/sample.json", "-v");
        
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
        var context = new BucketWorkerTestContext();
        var installWorker = context.GetBucketWorker("-i", "./Data/bundle.dap.tar.gz", "-o", "./temp");
        var stopWorker = context.GetBucketWorker("-t", "./temp/manifest.json");
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        await installWorker.StartAsync(CancellationToken.None);
        await stopWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.DownStackAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Execute_Remove_RemovalExecuted()
    {
        var context = new BucketWorkerTestContext();
        var installWorker = context.GetBucketWorker("-i", "./Data/bundle.dap.tar.gz", "-o", "./temp");
        var stopWorker = context.GetBucketWorker("-r", "./temp/manifest.json");
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        await installWorker.StartAsync(CancellationToken.None);
        await stopWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.DownStackAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.RemoveImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
    
    [Fact]
    public async Task Execute_Start_StartExecuted()
    {
        var context = new BucketWorkerTestContext();
        var installWorker = context.GetBucketWorker("-i", "./Data/bundle.dap.tar.gz", "-o", "./temp");
        var stopWorker = context.GetBucketWorker("-s", "./temp/manifest.json");
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        await installWorker.StartAsync(CancellationToken.None);
        await stopWorker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
        context.DockerService.Verify(v => v.UpStackAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
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