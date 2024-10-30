using CommandLine;

namespace Bucket.Service.Options;

public sealed class Actions
{
    [Option('b', "bundle", Required = false, HelpText = "Bundle given manifest, or try to find and bundle one in the current directory")]
    public bool Bundle { get; set; } = false;
    
    [Option('i', "install", Required = false, HelpText = "Install given bundle")]
    public bool Install { get; set; } = false;
    
    [Option('u', "update", Required = false, HelpText = "Update given bundle")]
    public bool Update { get; set; } = false;
    
    [Option('s', "start", Required = false, HelpText = "Start given bundle")]
    public bool Start { get; set; } = false;
    
    [Option('t', "stop", Required = false, HelpText = "Stops given bundle")]
    public bool Stop { get; set; } = false;

    [Option('m', "manifest", Required = false, HelpText = "Path to the manifest file")]
    public string ManifestFilePath { get; set; } = string.Empty;
    
    [Option('o', "output", Required = false, HelpText = "Path to the output file")]
    public string OutputFilePath { get; set; } = string.Empty;
}