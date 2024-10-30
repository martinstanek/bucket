using System.Collections.Generic;

namespace Bucket.Service.Model;

public sealed record BundleManifest
{
    public required Info Info { get; init; }

    public required Configuration Configuration { get; init; }

    public required IReadOnlyCollection<Parameter> Parameters { get; init; }

    public required IReadOnlyCollection<Registry> Registries { get; init; }

    public required IReadOnlyCollection<Image> Images { get; init; }

    public required IReadOnlyCollection<string> Stacks { get; init; }
}

public sealed record Info 
{
    public required string Name { get; init; }    

    public required string Description { get; init; }

    public required string Version { get; init; }
}

public sealed record Configuration
{
    public required bool FetchImages { get; init; }
}

public sealed record Image
{
    public required string Alias { get; init; }

    public required string FullName { get; init; }
}

public sealed record Registry
{
    public required string Name { get; init; }

    public required string Server { get; init; }

    public string User { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

public sealed record Parameter
{
    public required string Name { get; init;}

    public required string Description { get; init;}
}