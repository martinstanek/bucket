using Bucket.Host.Extensions;
using Bucket.Host.Output;
using Bucket.Service.Extensions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.SetDefaultLevels();
builder.Services.AddBucket(new ConsoleOutput(), args);
builder.Build().Run();