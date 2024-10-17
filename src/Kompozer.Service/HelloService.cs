using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Kompozer.Service;

public sealed class HelloService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Scanning for definitions ..");

        var workDir = AppContext.BaseDirectory;
        var files = Directory.GetFiles(workDir);

        foreach (var file in files)
        {
            Console.WriteLine(file);
        }

        return Task.CompletedTask;
    }
}