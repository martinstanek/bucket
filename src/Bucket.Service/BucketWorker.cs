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
        if (!_arguments.IsValid || _arguments.IsHelp)
        {
            Console.WriteLine(_arguments.GetHelp());
            
            _lifetime.StopApplication();
            
            return;
        }

        var argumentsValidation = new ArgumentsValidation(_arguments.GetOptions());

        if (argumentsValidation.IsBundleCommand(out var bundleManifestPath, out var bundleOutputPath))
        {
            await _bundleService.BundleAsync(bundleManifestPath, bundleOutputPath);
            
            _lifetime.StopApplication();
            
            return;
        }

        if (argumentsValidation.IsInstallCommand(out var installBundlePath))
        {
            await _bundleService.InstallAsync(installBundlePath);
            
            _lifetime.StopApplication();
            
            return;
        }

        if (argumentsValidation.IsStartCommand(out var startManifestPath))
        {
            await _bundleService.StartAsync(startManifestPath);
            
            _lifetime.StopApplication();
            
            return;
        }

        if (argumentsValidation.IsStopCommand(out var stopManifestPath))
        {
            await _bundleService.StopAsync(stopManifestPath);
            
            _lifetime.StopApplication();
            
            return;
        }
        
        Console.WriteLine("Arguments are invalid, please provide valid arguments.");
        Console.WriteLine(_arguments.GetHelp());
            
        _lifetime.StopApplication();
    }
}