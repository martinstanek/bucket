using System.Collections.Generic;

namespace Bucket.Service.Options;

public sealed class ArgumentsValidation
{
    private readonly IReadOnlyCollection<Argument> _options;

    public ArgumentsValidation(IReadOnlyCollection<Argument> options)
    {
        _options = options;
    }

    public bool IsBundleCommand(out string manifestPath, out string outputBundlePath)
    {
        manifestPath = string.Empty;
        outputBundlePath = string.Empty;
        
        return false;
    }
    
    public bool IsInstallCommand(out string outputBundlePath)
    {
        outputBundlePath = string.Empty;
        
        return false;
    }
    
    public bool IsStopCommand(out string manifestPath)
    {
        manifestPath = string.Empty;
        
        return false;
    }
    
    public bool IsStartCommand(out string manifestPath)
    {
        manifestPath = string.Empty;
        
        return false;
    }
}