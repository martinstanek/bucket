using Bucket.Service.Options;
using Bucket.Service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bucket.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBucket(this IServiceCollection services, string[] args)
    {
        var arguments = new Arguments(args)
            .AddArgument("b", "bundle", "Bundle given manifest")
            .AddArgument("i", "install", "Install given bundle")
            .AddArgument("u", "update", "Update given bundle")
            .AddArgument("s", "start", "Start given bundle")
            .AddArgument("t", "stop", "Stops given bundle")
            .AddArgument("m", "manifest", "Path to the manifest file", mustHaveValue: true)
            .AddArgument("o", "output", "Path to the output file", mustHaveValue: true);
        
        return services
            .AddSingleton(arguments)
            .AddSingleton<DockerService>()
            .AddSingleton<BundleService>()
            .AddSingleton<FileSystemService>()
            .AddHostedService<BucketWorker>();
    }
}