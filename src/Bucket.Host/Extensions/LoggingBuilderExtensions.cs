using Microsoft.Extensions.Logging;

namespace Bucket.Host.Extensions;

internal static class LoggingBuilderExtensions
{
    public static ILoggingBuilder SetDefaultLevels(this ILoggingBuilder builder)
    {
        builder.AddSimpleConsole(options =>
        {
            options.IncludeScopes = false;
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        });

        return builder.SetMinimumLevel(LogLevel.Warning);
    }
}