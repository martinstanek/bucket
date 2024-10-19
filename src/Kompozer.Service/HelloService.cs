using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Kompozer.Service.Docker;
using Kompozer.Service.Model;
using Kompozer.Service.Serialization;
using System.Diagnostics;

namespace Kompozer.Service;

public sealed class HelloService : BackgroundService
{
    private readonly DockersClient _dockerClient;

    public HelloService(DockersClient dockersClient)
    {
        _dockerClient = dockersClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!TryFindBundleDefinition(out var bundleDefinition) || bundleDefinition is null)
        {
            Console.WriteLine("No bundle definition has been found ...");
            return;
        }

        Console.WriteLine(await _dockerClient.GetVersionAsync());
        Console.WriteLine("Pulling images ...");

        foreach (var image in bundleDefinition.Images)
        {
            Console.WriteLine(await _dockerClient.PullImageAsync(image.FullName));
        }

        var exportDirectory = Path.Combine(AppContext.BaseDirectory, "_images");
        
        Directory.CreateDirectory(exportDirectory);

        Console.WriteLine($"Exporting images into: {exportDirectory}");

        foreach (var image in bundleDefinition.Images)
        {
            Console.WriteLine(await _dockerClient.ExportImageAsync(image.FullName, Path.Combine(exportDirectory, $"{image.Alias}.tar")));
        }
    }

    private static bool TryFindBundleDefinition(out BundleDefinition? definition)
    {
        definition = default;

        var workDir = AppContext.BaseDirectory;
        var files = Directory.GetFiles(workDir).Where(f => f.EndsWith("json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            if (TryParseBundleDefinition(file, out var bundleDefinition) && bundleDefinition is not null)
            {
                definition = bundleDefinition;
                return true;
            }
        }

        return false;
    }

    private static bool TryParseBundleDefinition(string path, out BundleDefinition? definition)
    {
        var content = File.ReadAllText(path);
        definition = default;

        try
        {
            definition = JsonSerializer.Deserialize(content, SourceGenerationContext.Default.BundleDefinition);

            return true;
        }
        catch
        {
            return false;
        }
    }
}