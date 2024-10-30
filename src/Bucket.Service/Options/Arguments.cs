using System.Collections.Generic;
using System.Text;

namespace Bucket.Service.Options;

public sealed class Arguments
{
    private readonly string[] _args;
    private readonly Argument[] _arguments = new[]
    {   
        new Argument("h", "help", "Shows help"),
        new Argument("b", "bundle", "Bundle given manifest, or try to find and bundle one in the current directory"),
        new Argument("i", "install", "Install given bundle"),
    };
    
    public Arguments(string[] args)
    {
        _args = args;
    }

    public IReadOnlyDictionary<Argument, string> GetOptions()
    {
        return new Dictionary<Argument, string>();
    }

    public string GetHelp()
    {
        var sb = new StringBuilder();

        foreach (var argument in _arguments)
        {
            sb.AppendLine(argument.ToString());
        }

        return sb.ToString();
    }

    public bool IsValid => true;

    /*
    [Option('u', "update", Required = false, HelpText = "Update given bundle")]
    [Option('s', "start", Required = false, HelpText = "Start given bundle")]
    [Option('t', "stop", Required = false, HelpText = "Stops given bundle")]
    [Option('m', "manifest", Required = false, HelpText = "Path to the manifest file")]
    [Option('o', "output", Required = false, HelpText = "Path to the output file")]
    */


}