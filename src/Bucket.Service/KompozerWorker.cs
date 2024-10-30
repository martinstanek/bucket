using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Kompozer.Service.Services;

namespace Kompozer.Service;

public sealed class KompozerWorker : BackgroundService
{
    private readonly BundleService _bundleService;
    private readonly IHostApplicationLifetime _lifetime;
    
    public KompozerWorker(BundleService bundleService, IHostApplicationLifetime lifetime)
    {
        _bundleService = bundleService;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _bundleService.CheckDockerAsync();
        
        if (!_bundleService.TryFindBundleManifest(out var manifest, out var manifestPath) || manifest is null)
        {
            Console.WriteLine("No bundle manifest has been found.");
            
            _lifetime.StopApplication();
            
            return;
        }

        Console.WriteLine(manifest.Info);

        await _bundleService.CreateBundleAsync(manifest, manifestPath, AppContext.BaseDirectory);

        Console.WriteLine("Done");

        _lifetime.StopApplication();
    }
}