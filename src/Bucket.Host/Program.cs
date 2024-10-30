using Microsoft.Extensions.Hosting;
using Kompozer.Host.Extensions;
using Kompozer.Service.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetDefaultLevels();
builder.Services.AddKompozer();
builder.Build().Run();