using Bucket.Service.Options;
using Bucket.Service.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bucket.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBucket(this IServiceCollection services, IOutput output, string[] args)
    {
        var arguments = new Arguments(args)
            .AddArgument("h", "help", "Show this help")
            .AddArgument("b", "bundle", "Bundle given manifest", "Either the manifest path is provided, or a valid manifest is searched in the current directory")
            .AddArgument("i", "install", "Install given bundle", "The path to the bundle is required", ArgumentValueRequirement.MustHave)
            .AddArgument("r", "remove", "Uninstall and remove given bundle", "The path to the bundle folder is required", ArgumentValueRequirement.MustHave)
            .AddArgument("s", "start", "Start given bundle", "The path to the bundle manifest is required", ArgumentValueRequirement.MustHave)
            .AddArgument("t", "stop", "Stop given bundle", "The path to the bundle folder is required", ArgumentValueRequirement.MustHave)
            .AddArgument("o", "output", "Path to the output file or directory", ArgumentValueRequirement.MustHave)
            .AddArgument("w", "workdir", "Path to the working directory during bundling", "If no directory provided, the current executable directory will be used", ArgumentValueRequirement.MustHave)
            .AddArgument("v", "verbose", "Turn on internal logs", ArgumentValueRequirement.CanNotHave);

        services
            .AddLogging()
            .AddSingleton(output)
            .AddSingleton(arguments)
            .AddSingleton<IDockerService, DockerService>()
            .AddSingleton<IBundleService, BundleService>()
            .AddSingleton<IFileSystemService, FileSystemService>()
            .AddSingleton<ICompressorService, CompressorService>()
            .AddSingleton<BucketWorker>()
            .AddHostedService(s => s.GetRequiredService<BucketWorker>());

        return arguments.ContainsOption("v")
            ? services
            : services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
    }
}