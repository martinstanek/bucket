using Bucket.Service.Options;
using Shouldly;
using Xunit;

namespace Bucket.Tests.Service.Options;

public sealed class ArgumentsTests
{
    [Fact]
    public void GetOptions_InputIsValid_ReturnsOptions()
    {
        var arguments = new Arguments("-i ./bundle.dap.tar.gz")
            .AddArgument("i", "install", "Install manifest");

        var options = arguments.GetOptions();
        var value = arguments.GetOptionValue("i");

        arguments.IsValid.ShouldBeTrue();
        arguments.IsHelp.ShouldBeFalse();
        value.ShouldBe("./bundle.dap.tar.gz");
        options.ShouldNotBeEmpty();
        options.Count.ShouldBe(1);
    }
    
    [Fact]
    public void GetOptions_InputIsValid_SingleOption_ReturnsOptions()
    {
        var arguments = new Arguments("--help")
            .AddArgument("h", "help", "Install manifest");

        var options = arguments.GetOptions();

        arguments.IsValid.ShouldBeTrue();
        arguments.IsHelp.ShouldBeTrue();
        options.Count().ShouldBe(1);
    }
    
    [Fact]
    public void GetOptions_InputContainsInValidOption_InvalidOptionIsIgnored()
    {
        var arguments = new Arguments("-i -m ./manifest.json -x")
            .AddArgument("i", "install", "Install manifest")
            .AddArgument("m", "manifest", "Path to the manifest file", mustHaveValue: true);

        var options = arguments.GetOptions();
        
        arguments.IsValid.ShouldBeTrue();
        options.Count.ShouldBe(2);
        options.ShouldNotContain(o => o.ShortName.Equals("x"));
    }
    
    [Fact]
    public void GetOptions_InputIsInValid_ReturnsEmptyArray()
    {
        var arguments = new Arguments("./manifest.json")
            .AddArgument("i", "install", "Install manifest")
            .AddArgument("m", "manifest", "Path to the manifest file", mustHaveValue: true);

        var options = arguments.GetOptions();
        var value = arguments.GetOptionValue("m");

        arguments.IsValid.ShouldBeFalse();
        value.ShouldBeEmpty();
        options.ShouldBeEmpty();
    }
    
    [Fact]
    public void GetOptions_InputIsEmpty_ReturnsEmptyArray()
    {
        var arguments = new Arguments("")
            .AddArgument("i", "install", "Install manifest")
            .AddArgument("m", "manifest", "Path to the manifest file", mustHaveValue: true);

        var options = arguments.GetOptions();

        arguments.IsValid.ShouldBeFalse();
        options.ShouldBeEmpty();
    }
    
    [Fact]
    public void GetOptions_InputIsInValid_NoValueProvided_ReturnsEmptyArray()
    {
        var arguments = new Arguments("-i -m")
            .AddArgument("i", "install", "Install manifest")
            .AddArgument("m", "manifest", "Path to the manifest file", mustHaveValue: true);

        var options = arguments.GetOptions();
        var value = arguments.GetOptionValue("m");

        arguments.IsValid.ShouldBeFalse();
        value.ShouldBeEmpty();
        options.ShouldBeEmpty();
    }
    
    [Fact]
    public void ContainsOption_InputIsInValid_ReturnsTrue()
    {
        var arguments = new Arguments("-i -m ./test.json")
            .AddArgument("i", "install", "Install manifest")
            .AddArgument("m", "manifest", "Path to the manifest file", mustHaveValue: true);

        arguments.ContainsOption("i").ShouldBeTrue();
        arguments.ContainsOption("m").ShouldBeTrue();
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