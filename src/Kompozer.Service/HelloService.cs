using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kompozer.Service.Docker;
using Kompozer.Service.Model;
using Kompozer.Service.Serialization;
using Microsoft.Extensions.Hosting;

namespace Kompozer.Service;

public sealed class HelloService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        FindAndReadDefinition();

        var dockerClient = new DockersClient();

        Console.WriteLine(await dockerClient.GetVersionAsync());
    }

    private static void FindAndReadDefinition()
    {
        Console.WriteLine("Scanning for definitions ..");

        var workDir = AppContext.BaseDirectory;
        var files = Directory.GetFiles(workDir)
            .Where(f => f.EndsWith("json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            if (TryParseBundleDefinition(file, out var definition) && definition is not null)
            {
                Console.WriteLine(definition.Info.Name);
                Console.WriteLine(definition.Info.Description);
            }
        }
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