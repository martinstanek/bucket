using System;
using System.Threading;
using System.Threading.Tasks;
using Bucket.Service.Options;
using Bucket.Service.Services;
using Microsoft.Extensions.Hosting;

namespace Bucket.Service;

public sealed class BucketWorker : BackgroundService
{
    private readonly BundleService _bundleService;
    private readonly Arguments _arguments;
    private readonly IHostApplicationLifetime _lifetime;
    
    public BucketWorker(BundleService bundleService, Arguments arguments, IHostApplicationLifetime lifetime)
    {
        _bundleService = bundleService;
        _arguments = arguments;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processorTask = new ArgumentsProcessor(_arguments.GetOptions())
            .WithInvalidArguments(HandleInvalidArgumentsAsync)
            .WithBundleCommand((manifestPath, outputPath) => _bundleService.BundleAsync(manifestPath, outputPath, stoppingToken))
            .WithInstallCommand(bundlePath => _bundleService.InstallAsync(bundlePath, stoppingToken))
            .WithStartCommand(manifestPath => _bundleService.StartAsync(manifestPath, stoppingToken))
            .WithStopCommand(manifestPath => _bundleService.StopAsync(manifestPath, stoppingToken))
            .Build();

        await processorTask;

        _lifetime.StopApplication();
    }

    private Task HandleInvalidArgumentsAsync(string message)
    {
        Console.WriteLine(message);
        Console.WriteLine(_arguments.GetHelp());
        
        return Task.CompletedTask;
    }
}