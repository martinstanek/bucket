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
    [Theory]
    [InlineData("-i", "./bundle.dap.tar.gz", "-o", "./test")]
    [InlineData("--install", "./bundle.dap.tar.gz", "--output", "./test")]
    [InlineData("--install", "./bundle.dap.tar.gz", "--output", "./test", "--verbose")]
    public async Task Execute_Install_InstallationExecuted(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.BundleAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        context.BundleService.Verify(v => v.InstallAsync(
            "./bundle.dap.tar.gz",
            "./test",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("-i", "./bundle.dap.tar.gz")]
    [InlineData("--install", "./bundle.dap.tar.gz")]
    [InlineData("-i", "-o", "./test")]
    [InlineData("--install", "--output", "./test")]
    [InlineData("-i", "./bundle.dap.tar.gz", "-o")]
    [InlineData("--install", "./bundle.dap.tar.gz", "--output")]
    public async Task Execute_Install_MissingArguments_InstallationNotExecuted(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.InstallAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("-b")]
    [InlineData("--bundle")]
    [InlineData("-b", "./manifest.json")]
    [InlineData("--bundle", "./manifest.json")]
    [InlineData("-b", "-o", "./output.dap.tar.gz")]
    [InlineData("--bundle", "-o", "./output.dap.tar.gz")]
    [InlineData("-b", "./manifest.json", "-o", "./output.dap.tar.gz")]
    [InlineData("--bundle", "./manifest.json", "-o", "./output.dap.tar.gz")]
    [InlineData("--bundle", "--output", "./output.dap.tar.gz")]
    [InlineData("-b", "./manifest.json", "--output", "./output.dap.tar.gz")]
    [InlineData("--bundle", "./manifest.json", "--output", "./output.dap.tar.gz")]
    [InlineData("--bundle", "./manifest.json", "--output", "./output.dap.tar.gz", "--verbose")]
    public async Task Execute_Bundle_BundlingExecuted(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.BundleAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("-b", "./manifest.json", "-o", "./output.dap.tar.gz")]
    [InlineData("--bundle", "./manifest.json", "--output", "./output.dap.tar.gz")]
    public async Task Execute_Bundle_PathsUsed(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.BundleAsync(
            "./manifest.json",
            "./output.dap.tar.gz",
            string.Empty,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("-b", "./manifest.json", "-o", "./output.dap.tar.gz", "-w", "./wrk")]
    [InlineData("--bundle", "./manifest.json", "--output", "./output.dap.tar.gz", "--workdir", "./wrk")]
    [InlineData("--bundle", "./manifest.json", "--output", "./output.dap.tar.gz", "--verbose", "--workdir", "./wrk")]
    public async Task Execute_Bundle_WorkdirUsed(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.BundleAsync(
            "./manifest.json",
            "./output.dap.tar.gz",
            "./wrk",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("-r", "./bundle")]
    [InlineData("--remove", "./bundle")]
    public async Task Execute_Uninstall_UninstallExecuted(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.RemoveAsync(
            "./bundle",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("-r")]
    [InlineData("--remove")]
    public async Task Execute_Uninstall_BundlePathNotProvided_UninstallNotExecuted(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.RemoveAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("-s", "./bundle/manifest.json")]
    [InlineData("--start", "./bundle/manifest.json")]
    public async Task Execute_Start_StartExecuted(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.StartAsync(
            "./bundle/manifest.json",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("-s")]
    [InlineData("--start")]
    [InlineData("-S")]
    [InlineData("--Start")]
    public async Task Execute_Start_BundlePathNotProvided_StartNotExecuted(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.StartAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("-t", "./bundle")]
    [InlineData("--stop", "./bundle")]
    public async Task Execute_Stop_StopExecuted(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.StopAsync(
            "./bundle",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("-t")]
    [InlineData("--stop")]
    public async Task Execute_Stop_BundlePathNotProvided_StopNotExecuted(params string[] args)
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker(args);

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.StopAsync(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Execute_NoArguments_NothingIsCalled()
    {
        var context = new BucketWorkerTestContext();
        var worker = context.GetBucketWorker();

        await worker.StartAsync(CancellationToken.None);

        context.HostLifeTime.Verify(v => v.StopApplication(), Times.Once);
        context.BundleService.Verify(v => v.InstallAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        context.BundleService.Verify(v => v.StopAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        context.BundleService.Verify(v => v.StartAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        context.BundleService.Verify(v => v.BundleAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private sealed class BucketWorkerTestContext
    {
        public BucketWorker GetBucketWorker(params string[] args)
        {
            var services = new ServiceCollection()
                .AddBucket(new Mock<IOutput>().Object, args)
                .RemoveAll<IHostApplicationLifetime>()
                .RemoveAll<IDockerService>()
                .RemoveAll<IFileSystemService>()
                .RemoveAll<IBundleService>()
                .AddSingleton(HostLifeTime.Object)
                .AddSingleton(DockerService.Object)
                .AddSingleton(FileSystemService.Object)
                .AddSingleton(BundleService.Object);

            return services
                .BuildServiceProvider()
                .GetRequiredService<BucketWorker>();
        }

        internal Mock<IHostApplicationLifetime> HostLifeTime { get; } = new();

        internal Mock<IBundleService> BundleService { get; } = new();

        private Mock<IDockerService> DockerService { get; } = new();

        private Mock<IFileSystemService> FileSystemService { get; } = new();

    }
}