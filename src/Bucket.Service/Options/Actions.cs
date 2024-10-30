using CommandLine;

namespace Bucket.Service.Options;

public class Actions
{
    [Option('b', "bundle", Required = false, HelpText = "Bundle given manifest, or try to find and bundle one in the current directory")]
    public bool Bundle { get; init; } = false;
    
    [Option('i', "install", Required = false, HelpText = "Install given bundle")]
    public bool Install { get; init; } = false;
    
    [Option('u', "update", Required = false, HelpText = "Update given bundle")]
    public bool Update { get; init; } = false;
    
    [Option('s', "start", Required = false, HelpText = "Start given bundle")]
    public bool Start { get; init; } = false;
    
    [Option('t', "stop", Required = false, HelpText = "Stops given bundle")]
    public bool Stop { get; init; } = false;

    [Option('m', "manifest", Required = false, HelpText = "Path to the manifest file")]
    public string ManifestFilePath { get; init; } = string.Empty;
    
    [Option('o', "output", Required = false, HelpText = "Path to the output file")]
    public string OutputFilePath { get; init; } = string.Empty;
}