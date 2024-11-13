﻿using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bucket.Service.Options;
using Bucket.Service.Services;
using Microsoft.Extensions.Hosting;

namespace Bucket.Service;

public sealed class BucketWorker : IHostedService
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

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        var action = new ActionBuilder(_arguments)
            .WithBundleCommand((manifestPath, outputPath) => _bundleService.BundleAsync(manifestPath, outputPath, string.Empty, stoppingToken))
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
                ? e.Message
                : e.ToString();
            
            Console.WriteLine($"Unexpected error: {error}");
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