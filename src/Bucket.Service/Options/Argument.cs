namespace Bucket.Service.Options;

public sealed record Argument(
    string ShortName,
    string FullName,
    string Description,
    string Note,
    bool MustHaveValue = false)
{
    public string Value { get; init; } = string.Empty;
}