using Kompozer.Service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kompozer.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKompozer(this IServiceCollection services)
    {
        return services
            .AddSingleton<DockerService>()
            .AddSingleton<BundleService>()
            .AddSingleton<FileSystemService>()
            .AddHostedService<KompozerWorker>();
    }
}