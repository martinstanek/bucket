﻿using Bucket.Service.Options;
using Bucket.Service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Bucket.Service.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBucket(this IServiceCollection services, string[] args)
    {
        var arguments = new Arguments(args)
            .AddArgument("h", "help", "Show this help")
            .AddArgument("b", "bundle", "Bundle given manifest", "Either the manifest path is provided, or a valid manifest is searched in the current directory")
            .AddArgument("i", "install", "Install given bundle", "The path to the bundle is required", mustHaveValue: true)
            .AddArgument("u", "uninstall", "Uninstall given bundle", "The path to the bundle folder is required", mustHaveValue: true)
            .AddArgument("s", "start", "Start given bundle", "The path to the bundle folder is required", mustHaveValue: true)
            .AddArgument("t", "stop", "Stop given bundle", "The path to the bundle folder is required", mustHaveValue: true)
            .AddArgument("o", "output", "Path to the output file or directory", mustHaveValue: true);
        
        return services
            .AddSingleton(arguments)
            .AddSingleton<IDockerService, DockerService>()
            .AddSingleton<IBundleService, BundleService>()
            .AddSingleton<IFileSystemService, FileSystemService>()
            .AddSingleton<ICompressorService, CompressorService>()
            .AddSingleton<BucketWorker>()
            .AddHostedService(s => s.GetRequiredService<BucketWorker>());
    }
}