using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Bucket.Service.Options;
using Bucket.Service.Services;

namespace Bucket.Service;

public sealed class BucketWorker : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IBundleService _bundleService;
    private readonly ILogger<BucketWorker> _logger;
    private readonly Arguments _arguments;

    public BucketWorker(
        IHostApplicationLifetime lifetime,
        IBundleService bundleService,
        Arguments arguments,
        ILogger<BucketWorker> logger)
    {
        _bundleService = bundleService;
        _arguments = arguments;
        _lifetime = lifetime;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        var action = new ActionBuilder(_arguments)
            .WithBundleCommand((manifestPath, outputPath, workingDir) => _bundleService.BundleAsync(manifestPath, outputPath, workingDir, stoppingToken))
            .WithInstallCommand((bundlePath, outputDirectory) => _bundleService.InstallAsync(bundlePath, outputDirectory, stoppingToken))
            .WithRemoveCommand(bundlePath => _bundleService.RemoveAsync(bundlePath, stoppingToken))
            .WithStartCommand(manifestPath => _bundleService.StartAsync(manifestPath, stoppingToken))
            .WithStopCommand(manifestPath => _bundleService.StopAsync(manifestPath, stoppingToken))
            .WithHelpCommand(HandleHelpAsync)
            .WithInvalidArguments(HandleInvalidArgumentsAsync)
            .Build();
        
        try
        {
            await action();
        }
        catch (Exception e)
        {
            var error = _arguments.ContainsOption("v")
                ? e.ToString()
                : e.Message;
            
            Console.WriteLine($"Unexpected error: {error}");
            
            _logger.LogError(e, e.Message);
        }
        
        _lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _lifetime.StopApplication();

        return Task.CompletedTask;
    }
    
    private Task HandleInvalidArgumentsAsync(string message)
    {
        return Task.Run(() =>
        {
            Console.WriteLine(message);
            Console.WriteLine(_arguments.GetHelp());    
        });
    }
    
    private Task HandleHelpAsync()
    {
        return Task.Run(() =>
        { 
            Console.WriteLine($"bucket by martinstanek {Assembly.GetEntryAssembly()?.GetName().Version}");
            Console.WriteLine(_arguments.GetHelp());    
        });
    }
}