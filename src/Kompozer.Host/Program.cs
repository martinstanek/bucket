using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Kompozer.Service;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<HelloService>();

var host = builder.Build();

host.Run();