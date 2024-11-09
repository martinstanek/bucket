using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bucket.Service.Options;

public sealed class ArgumentsProcessor
{
    private readonly IReadOnlyCollection<Argument> _options;
    private Task _taskToProcess = Task.CompletedTask;
    private Task _invalidArgumentsTask = Task.CompletedTask;

    public ArgumentsProcessor(IReadOnlyCollection<Argument> options)
    {
        _options = options;
    }

    public ArgumentsProcessor WithInvalidArguments(Func<string, Task> onInvalidArguments)
    {
        _invalidArgumentsTask = onInvalidArguments("Invalid arguments");
        
        return this;
    }

    public ArgumentsProcessor WithBundleCommand(Func<string, string, Task> onBundleCommand)
    {
        if (IsBundleCommand(out var manifestPath, out var outputBundlePath))
        {
            _taskToProcess = onBundleCommand(manifestPath, outputBundlePath);
        }

        return this;
    }

    public ArgumentsProcessor WithInstallCommand(Func<string, Task> onInstallCommand)
    {
        if (IsInstallCommand(out var manifestPath))
        {
            _taskToProcess = onInstallCommand(manifestPath);
        }

        return this;
    }

    public ArgumentsProcessor WithStopCommand(Func<string, Task> onStopCommand)
    {
        if (IsStopCommand(out var manifestPath))
        {
            _taskToProcess = onStopCommand(manifestPath);
        }

        return this;
    }

    public ArgumentsProcessor WithStartCommand(Func<string, Task> onStartCommand)
    {
        if (IsStartCommand(out var manifestPath))
        {
            _taskToProcess = onStartCommand(manifestPath);
        }

        return this;
    }

    public Task Build()
    {
        return _taskToProcess.IsCompleted 
            ? _invalidArgumentsTask 
            : _taskToProcess;
    }

    private bool IsBundleCommand(out string manifestPath, out string outputBundlePath)
    {
        manifestPath = string.Empty;
        outputBundlePath = string.Empty;
        
        return false;
    }
    
    private bool IsInstallCommand(out string outputBundlePath)
    {
        outputBundlePath = string.Empty;
        
        return false;
    }
    
    private bool IsStopCommand(out string manifestPath)
    {
        manifestPath = string.Empty;
        
        return false;
    }
    
    private bool IsStartCommand(out string manifestPath)
    {
        manifestPath = string.Empty;
        
        return false;
    }
}