using Bucket.Service;
using Bucket.Service.Extensions;
using Bucket.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Bucket.Tests.Service;

public sealed class BucketWorkerEndToEndTests
{
    [Fact]
    public async Task Execute_Install_InstallationExecuted()
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker("-i", "./bundle.dap.tar.gz", "-o", "./temp");
        
        context.DockerService.Setup(s => s.IsDockerRunningAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        await worker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.DockerService.Verify(v => v.IsDockerRunningAsync(It.IsAny<CancellationToken>()), Times.Once);
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
    
    private sealed class BucketWorkerTestContext
    {
        public BucketWorker GetBucketWorker(params string[] args)
        {
            var services = new ServiceCollection()
                .AddBucket(args)
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