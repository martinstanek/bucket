using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Kompozer.Service.Model;
using Kompozer.Service.Serialization;
using Microsoft.Extensions.Hosting;

namespace Kompozer.Service;

public sealed class HelloService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Scanning for definitions ..");

        var workDir = AppContext.BaseDirectory;
        var files = Directory.GetFiles(workDir)
            .Where(f => f.EndsWith("json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            Console.WriteLine(file);

            if (TryParseBundleDefinition(file, out var definition) && definition is not null)
            {
                Console.WriteLine(definition.Info.Name);
                Console.WriteLine(definition.Info.Description);
            }
        }


        return Task.CompletedTask;
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
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            
            return false;
        }
    }
}