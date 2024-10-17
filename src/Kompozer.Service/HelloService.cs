﻿using Microsoft.Extensions.Hosting;

namespace Kompozer.Service;

public sealed class HelloService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Hello World");

        return Task.CompletedTask;
    }
}