using Bucket.Service.Options;
using Shouldly;
using Xunit;

namespace Bucket.Tests.Service.Options;

public class ArgumentsTests
{
    [Fact]
    public void GetOptions_InputIsValid_ReturnsOptions()
    {
        var arguments = new Arguments("-i -m ./manifest.json")
            .AddArgument("i", "install", "Install manifest")
            .AddArgument("m", "manifest", "Path to the manifest file");

        var options = arguments.GetOptions();
        var value = arguments.GetOptionValue("m");

        arguments.IsValid.ShouldBeTrue();
        value.ShouldBe("./manifest.json");
        options.ShouldNotBeEmpty();
        options.Count.ShouldBe(2);
    }
}