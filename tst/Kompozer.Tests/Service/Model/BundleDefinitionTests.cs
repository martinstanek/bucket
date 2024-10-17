using Kompozer.Service.Model;
using Shouldly;
using System.Text.Json;
using Xunit;

namespace Kompozer.Tests.Service.Model;

public sealed class BundleDefinitionTests
{
    [Fact]
    public void Test()
    {
        var content = File.ReadAllText("Data/sample.json");

        var definition = JsonSerializer.Deserialize<BundleDefinition>(content);

        definition.ShouldNotBeNull().Info.Name.ShouldBe("TestBundle");
    }
}
