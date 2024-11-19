using System;
using System.Threading.Tasks;

namespace Bucket.Service.Options;

public sealed class ActionBuilder
{
    private readonly Arguments _arguments;
    private Func<Task>? _taskToProcess;
    private Func<Task>? _taskToFallback;

    public ActionBuilder(Arguments arguments)
    {
        _arguments = arguments;
    }

    public ActionBuilder WithInvalidArguments(Func<string, Task> onInvalidArguments)
    {
        _taskToFallback = () => onInvalidArguments("Invalid arguments");
        
        return this;
    }

    public ActionBuilder WithBundleCommand(Func<string, string, string, Task> onBundleCommand)
    {
        if (IsBundleCommand(out var manifestPath, out var outputBundlePath, out var workingDirectory))
        {
            _taskToProcess = () => onBundleCommand(manifestPath, outputBundlePath, workingDirectory);
        }

        return this;
    }

    public ActionBuilder WithInstallCommand(Func<string, string, Task> onInstallCommand)
    {
        if (IsInstallCommand(out var bundlePath, out var outputDirectory))
        {
            _taskToProcess = () => onInstallCommand(bundlePath, outputDirectory);
        }

        return this;
    }
    
    public ActionBuilder WithRemoveCommand(Func<string, Task> onUninstallCommand)
    {
        if (IsRemoveCommand(out var bundleFolderPath))
        {
            _taskToProcess = () => onUninstallCommand(bundleFolderPath);
        }

        return this;
    }

    public ActionBuilder WithStopCommand(Func<string, Task> onStopCommand)
    {
        if (IsStopCommand(out var manifestPath))
        {
            _taskToProcess = () => onStopCommand(manifestPath);
        }

        return this;
    }

    public ActionBuilder WithStartCommand(Func<string, Task> onStartCommand)
    {
        if (IsStartCommand(out var bundleFolderPath))
        {
            _taskToProcess = () => onStartCommand(bundleFolderPath);
        }

        return this;
    }

    public ActionBuilder WithHelpCommand(Func<Task> onHelpCommand)
    {
        if (IsHelpCommand())
        {
            _taskToProcess = onHelpCommand;
        }

        return this;
    }

    public Func<Task> Build()
    {
        return _taskToProcess 
               ?? _taskToFallback 
               ?? (() => Task.CompletedTask);
    }

    private bool IsBundleCommand(out string manifestPath, out string outputBundlePath, out string workingDirectory)
    {
        manifestPath = _arguments.GetOptionValue("b");
        outputBundlePath = _arguments.GetOptionValue("o");
        workingDirectory = _arguments.GetOptionValue("w");
       
        return _arguments.ContainsOptions(exclusively: ["b"], optionally: ["o", "v", "d", "w"]);
    }

    private bool IsInstallCommand(out string bundlePath, out string outputDirectory)
    {
        bundlePath = string.Empty;
        outputDirectory = string.Empty;

        if (!_arguments.ContainsOptions(["i", "o"], optionally: ["v"]))
        {
            return false;
        }
        
        bundlePath = _arguments.GetOptionValue("i");
        outputDirectory = _arguments.GetOptionValue("o");

        return true;

    }

    private bool IsRemoveCommand(out string bundleFolderPath)
    {
        return IsSingleOptionCommandWithValue("r", out bundleFolderPath);
    }

    private bool IsStopCommand(out string bundleFolderPath)
    {
        return IsSingleOptionCommandWithValue("t", out bundleFolderPath);
    }
    
    private bool IsStartCommand(out string bundleFolderPath)
    {
        return IsSingleOptionCommandWithValue("s", out bundleFolderPath);
    }

    private bool IsHelpCommand()
    {
        return _arguments.ContainsOptions(exclusively: ["h"]);
    }

    private bool IsSingleOptionCommandWithValue(string option, out string bundleFolderPath)
    {
        var valid = _arguments.ContainsOptions(exclusively: [option], optionally: ["v"])
                    && !string.IsNullOrWhiteSpace(_arguments.GetOptionValue(option));
        
        bundleFolderPath = valid 
            ? _arguments.GetOptionValue(option)
            : string.Empty;
        
        return valid;
    }
}