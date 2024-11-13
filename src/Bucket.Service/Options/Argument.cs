namespace Bucket.Service.Options;

public sealed record Argument(
    string ShortName,
    string FullName,
    string Description,
    string Note,
    ArgumentValueRequirement ValueRequirement = ArgumentValueRequirement.Optional)
{
    public string Value { get; init; } = string.Empty;
}

public enum ArgumentValueRequirement
{
    MustHave,
    CanNotHave,
    Optional
}