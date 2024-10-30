using System.Text.Json;
using Bucket.Service.Model;
using Shouldly;
using Xunit;

namespace Bucket.Tests.Service.Model;

public sealed class BundleManifestTests
{
    [Fact]
    public void Test()
    {
        var content = File.ReadAllText("Data/sample.json");

        var definition = JsonSerializer.Deserialize<BundleManifest>(content);

        definition.ShouldNotBeNull();
        definition.Info.Name.ShouldBe("TestBundle");
        definition.Info.Description.ShouldNotBeEmpty();
        definition.Configuration.FetchImages.ShouldBeTrue();
        definition.Info.Version.ShouldNotBeEmpty();
        definition.Images.ShouldNotBeEmpty();
        definition.Parameters.ShouldNotBeEmpty();
        definition.Registries.ShouldNotBeEmpty();
        definition.Stacks.ShouldNotBeEmpty();
    }
}