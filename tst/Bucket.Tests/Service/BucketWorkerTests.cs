using Bucket.Service;
using Bucket.Service.Extensions;
using Bucket.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace Bucket.Tests.Service;

public sealed class BucketWorkerTests
{
    [Fact]
    public async Task Execute_BucketServiceIsResoled()
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker();
        
        await worker.StartAsync(CancellationToken.None);
        
        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
    }

    private sealed class BucketWorkerTestContext
    {
        public BucketWorker GetBucketWorker(params string[] args)
        {
            var services = new ServiceCollection().AddBucket(args);

            services.RemoveAll<IHostApplicationLifetime>();
            services.RemoveAll<IDockerService>();
            services.RemoveAll<IFileSystemService>();
            services.RemoveAll<IBundleService>();
            services.AddSingleton(HostLifeTime.Object);
            services.AddSingleton(DockerService.Object);
            services.AddSingleton(FileSystemService.Object);
            services.AddSingleton(BundleService.Object);
            
            var provider = services.BuildServiceProvider();
            
            return provider.GetRequiredService<BucketWorker>();
        }

        internal Mock<IHostApplicationLifetime> HostLifeTime { get; } = new();
        
        internal Mock<IDockerService> DockerService { get; } = new();
        
        internal Mock<IFileSystemService> FileSystemService { get; } = new();
        
        internal Mock<IBundleService> BundleService { get; } = new();
    }
}