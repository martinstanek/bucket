using Bucket.Host.Extensions;
using Bucket.Service.Extensions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetDefaultLevels();
builder.Services.AddKompozer();
builder.Build().Run();