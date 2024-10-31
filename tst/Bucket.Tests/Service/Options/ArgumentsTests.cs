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
            .AddArgument("m", "manifest", "Path to the manifest file", mustHaveValue: true);

        var options = arguments.GetOptions();
        var value = arguments.GetOptionValue("m");

        arguments.IsValid.ShouldBeTrue();
        value.ShouldBe("./manifest.json");
        options.ShouldNotBeEmpty();
        options.Count.ShouldBe(2);
    }
    
    [Fact]
    public void AddArgument_ArgumentShortNameAlreadyExists_ThrowsInvalidOperationException()
    {
        Should.Throw<InvalidOperationException>(() => new Arguments("-i -m ./manifest.json")
            .AddArgument("i", "install", "Install manifest")
            .AddArgument("i", "installation", "Install manifest")
            .AddArgument("m", "manifest", "Path to the manifest file"));
    }
    
    [Fact]
    public void AddArgument_ArgumentFullNameAlreadyExists_ThrowsInvalidOperationException()
    {
        Should.Throw<InvalidOperationException>(() => new Arguments("-i -m ./manifest.json")
            .AddArgument("i", "install", "Install manifest")
            .AddArgument("x", "install", "Install manifest again")
            .AddArgument("m", "manifest", "Path to the manifest file"));
    }
}