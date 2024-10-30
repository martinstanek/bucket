using System;
using System.Threading;
using System.Threading.Tasks;
using Bucket.Service.Services;
using Microsoft.Extensions.Hosting;

namespace Bucket.Service;

public sealed class BucketWorker : BackgroundService
{
    private readonly BundleService _bundleService;
    private readonly IHostApplicationLifetime _lifetime;
    
    public BucketWorker(BundleService bundleService, IHostApplicationLifetime lifetime)
    {
        _bundleService = bundleService;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await _bundleService.IsDockerRunningAsync())
        {
            _lifetime.StopApplication();
            
            return;
        }

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