using System;
using System.Threading;
using System.Threading.Tasks;
using Bucket.Service.Exceptions;
using Bucket.Service.Options;
using Bucket.Service.Services;
using Microsoft.Extensions.Hosting;

namespace Bucket.Service;

public sealed class BucketWorker : BackgroundService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IBundleService _bundleService;
    private readonly Arguments _arguments;
    
    public BucketWorker(IHostApplicationLifetime lifetime, IBundleService bundleService, Arguments arguments)
    {
        _bundleService = bundleService;
        _arguments = arguments;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var action = new ActionBuilder(_arguments)
            .WithBundleCommand((manifestPath, outputPath) => _bundleService.BundleAsync(manifestPath, outputPath, stoppingToken))
            .WithInstallCommand(bundlePath => _bundleService.InstallAsync(bundlePath, stoppingToken))
            .WithUninstallCommand(bundlePath => _bundleService.UninstallAsync(bundlePath, stoppingToken))
            .WithStartCommand(manifestPath => _bundleService.StartAsync(manifestPath, stoppingToken))
            .WithStopCommand(manifestPath => _bundleService.StopAsync(manifestPath, stoppingToken))
            .WithInvalidArguments(HandleInvalidArgumentsAsync)
            .Build();

        try
        {
            await action;
        }
        catch (BucketException be)
        {
            Console.WriteLine(be.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
        
        _lifetime.StopApplication();
    }

    private Task HandleInvalidArgumentsAsync(string message)
    {
        Console.WriteLine(message);
        Console.WriteLine(_arguments.GetHelp());
        
        return Task.CompletedTask;
    }
}