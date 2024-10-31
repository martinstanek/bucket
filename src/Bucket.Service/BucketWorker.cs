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