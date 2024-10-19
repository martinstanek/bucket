using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Kompozer.Service;
using Kompozer.Service.Docker;
using Kompozer.Host.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetDefaultLevels();
builder.Services.AddSingleton<DockerClient>();
builder.Services.AddHostedService<HelloService>();

var host = builder.Build();

host.Run();