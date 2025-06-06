﻿using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Ardalis.GuardClauses;
using Bucket.Service.Serialization;

namespace Bucket.Service.Model;

public sealed record BundleManifest
{
    public required Info Info { get; init; }

    public required Configuration Configuration { get; init; }

    public required IReadOnlyCollection<Image> Images { get; init; }

    public required IReadOnlyCollection<string> Stacks { get; init; }

    public static BundleManifest Empty => new()
    {
        Info = Info.Empty,
        Configuration = Configuration.Empty,
        Images = [],
        Stacks = []
    };

    public static bool TryParseFromPath(string manifestPath, out BundleManifest? definition)
    {
        Guard.Against.NullOrWhiteSpace(manifestPath);

        var content = File.ReadAllText(manifestPath);
        definition = default;

        try
        {
            definition = JsonSerializer.Deserialize(content, SourceGenerationContext.Default.BundleManifest);

            return true;
        }
        catch
        {
            return false;
        }
    }
}

public sealed record Info
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public static Info Empty => new()
    {
         Name = string.Empty,
         Description = string.Empty,
         Version = string.Empty
    };
}

public sealed record Configuration
{
    public required bool FetchImages { get; init; }

    public static Configuration Empty => new()
    {
        FetchImages = false
    };
}

public sealed record Image
{
    public required string Alias { get; init; }

    public required string FullName { get; init; }
}