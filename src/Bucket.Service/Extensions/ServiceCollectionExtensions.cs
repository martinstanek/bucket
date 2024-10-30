using Bucket.Service.Options;
using Bucket.Service.Services;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Bucket.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBucket(this IServiceCollection services, string[] args)
    {
        var actions = Parser.Default.ParseArguments<Actions>(args);
        
        return services
            .AddSingleton(actions)
            .AddSingleton<DockerService>()
            .AddSingleton<BundleService>()
            .AddSingleton<FileSystemService>()
            .AddHostedService<BucketWorker>();
    }
}