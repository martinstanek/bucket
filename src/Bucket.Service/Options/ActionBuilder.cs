using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bucket.Service.Options;

public sealed class ActionBuilder
{
    private readonly Arguments _arguments;
    private readonly IReadOnlyCollection<Argument> _options;
    private Task _invalidArgumentsTask = Task.CompletedTask;
    private Task? _taskToProcess;

    public ActionBuilder(Arguments arguments)
    {
        _arguments = arguments;
        _options = _arguments.GetOptions();
    }

    public ActionBuilder WithInvalidArguments(Func<string, Task> onInvalidArguments)
    {
        _invalidArgumentsTask = onInvalidArguments("Invalid arguments");
        
        return this;
    }

    public ActionBuilder WithBundleCommand(Func<string, string, Task> onBundleCommand)
    {
        if (IsBundleCommand(out var manifestPath, out var outputBundlePath))
        {
            _taskToProcess = onBundleCommand(manifestPath, outputBundlePath);
        }

        return this;
    }

    public ActionBuilder WithInstallCommand(Func<string, Task> onInstallCommand)
    {
        if (IsInstallCommand(out var manifestPath))
        {
            _taskToProcess = onInstallCommand(manifestPath);
        }

        return this;
    }
    
    public ActionBuilder WithUninstallCommand(Func<string, Task> onUninstallCommand)
    {
        if (IsUninstallCommand(out var bundleFolderPath))
        {
            _taskToProcess = onUninstallCommand(bundleFolderPath);
        }

        return this;
    }

    public ActionBuilder WithStopCommand(Func<string, Task> onStopCommand)
    {
        if (IsStopCommand(out var manifestPath))
        {
            _taskToProcess = onStopCommand(manifestPath);
        }

        return this;
    }

    public ActionBuilder WithStartCommand(Func<string, Task> onStartCommand)
    {
        if (IsStartCommand(out var bundleFolderPath))
        {
            _taskToProcess = onStartCommand(bundleFolderPath);
        }

        return this;
    }

    public Task Build()
    {
        return _taskToProcess ?? _invalidArgumentsTask;
    }

    private bool IsBundleCommand(out string manifestPath, out string outputBundlePath)
    {
        manifestPath = string.Empty;
        outputBundlePath = string.Empty;
        
        var valid = _options.Count is > 0 and < 3 
                    && _arguments.ContainsOption("b");

        if (valid && _options.Count is 2)
        {
            valid = _arguments.ContainsOption("o") 
                    && !string.IsNullOrWhiteSpace(_arguments.GetOptionValue("o"));
            
            outputBundlePath = _arguments.GetOptionValue("o");
        }

        manifestPath = valid 
            ? _arguments.GetOptionValue("b")
            : string.Empty;
        
        return valid;
    }
    
    private bool IsInstallCommand(out string bundleFolderPath)
    {
        var valid = _options.Count == 1 && _arguments.ContainsOption("i");
        
        bundleFolderPath = valid 
            ? _arguments.GetOptionValue("i")
            : string.Empty;
        
        return valid;
    }

    private bool IsUninstallCommand(out string bundleFolderPath)
    {
        return IsSingleOptionCommandWithValue("u", out bundleFolderPath);
    }

    private bool IsStopCommand(out string bundleFolderPath)
    {
        return IsSingleOptionCommandWithValue("t", out bundleFolderPath);
    }
    
    private bool IsStartCommand(out string bundleFolderPath)
    {
        return IsSingleOptionCommandWithValue("s", out bundleFolderPath);
    }
    
    private bool IsSingleOptionCommandWithValue(string option, out string bundleFolderPath)
    {
        var valid = _options.Count == 1
                    && _arguments.ContainsOption(option)
                    && !string.IsNullOrWhiteSpace(_arguments.GetOptionValue(option));
        
        bundleFolderPath = valid 
            ? _arguments.GetOptionValue(option)
            : string.Empty;
        
        return valid;
    }
}