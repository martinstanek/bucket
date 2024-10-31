namespace Bucket.Service.Options;

public sealed record Argument(
    string ShortName,
    string FullName,
    string Description)
{
    public string Value { get; init; } = string.Empty;
}