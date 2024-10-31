namespace Bucket.Service.Options;

public sealed record Argument(
    string ShortName,
    string FullName,
    string Description,
    bool MustHaveValue = false)
{
    public string Value { get; init; } = string.Empty;
}