using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Ardalis.GuardClauses;

namespace Bucket.Service.Options;

public sealed class Arguments
{
    private readonly List<Argument> _arguments = [];
    private readonly List<Argument> _options = [];
    private readonly LinkedList<string> _args;
    private bool _isValid;
    
    public Arguments(string args)
    {
        var arguments = args.Split(' ');
        var trimmed = arguments.Select(s => s.Trim());
        
        _args = new LinkedList<string>(trimmed);
    }

    public Arguments(string[] args)
    {
        _args = new LinkedList<string>(args);
    }

    public Arguments AddArgument(string shortName, string fullName, string description, string note = "", bool mustHaveValue = false)
    {
        Guard.Against.NullOrWhiteSpace(shortName);
        Guard.Against.NullOrWhiteSpace(fullName);
        Guard.Against.NullOrWhiteSpace(description);
        
        var argument = new Argument(shortName, fullName, description, note, mustHaveValue);
        
        return AddArgument(argument);
    }
    
    public Arguments AddArgument(Argument argument)
    {
        if (_arguments.Any(a =>
                a.ShortName.Equals(argument.ShortName, StringComparison.OrdinalIgnoreCase)
                || a.FullName.Equals(argument.FullName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("The argument is already added.");
        }

        _arguments.Add(argument);    
        
        return this;
    }

    public IReadOnlyCollection<Argument> GetOptions()
    {
        _isValid = false;
        _options.Clear();
        
        for (var argNode = _args.First; argNode is not null; argNode = argNode.Next)
        {
            if (!IsOption(argNode.Value, out var option) || option is null)
            {
                continue;
            }
            
            if (option.MustHaveValue && (argNode.Next is null || !IsValue(argNode.Next.Value)))
            {
                _isValid = false;
                _options.Clear();
                    
                return Array.Empty<Argument>();
            }
            
            if (argNode.Next is not null && IsValue(argNode.Next.Value))
            {
                _options.Add(option with { Value = argNode.Next.Value });

                continue;
            }
                
            _options.Add(option);
        }
        
        _isValid = _options.Any();

        return _options.ToArray();
    }

    public string GetOptionValue(string shortName)
    {
        Guard.Against.NullOrWhiteSpace(shortName);
        
        var option = _options.FirstOrDefault(a => a.ShortName.Equals(shortName, StringComparison.OrdinalIgnoreCase));
        
        return option?.Value ?? string.Empty;
    }

    public string GetHelp()
    {  
        var sb = new StringBuilder();

        sb.AppendLine("Arguments:");
        
        foreach (var argument in _arguments)
        {
            sb.AppendLine($"    -{argument.ShortName}, --{argument.FullName} : {argument.Description}");
            
            if (!string.IsNullOrEmpty(argument.Note))
            {
                sb.AppendLine($"        {argument.Note}");
            }
        }

        return sb.ToString();
    }

    public bool ContainsOption(string shortName)
    {
        Guard.Against.NullOrWhiteSpace(shortName);

        return IsOption($"-{shortName}", out _);
    }

    private bool IsOption(string arg, out Argument? argument)
    {
        var shortNameArgument = _arguments.SingleOrDefault(a => $"-{a.ShortName}".Equals(arg, StringComparison.OrdinalIgnoreCase));
        var fullNameArgument = _arguments.SingleOrDefault(a => $"--{a.FullName}".Equals(arg, StringComparison.OrdinalIgnoreCase));
        
        argument = shortNameArgument ?? fullNameArgument;

        return shortNameArgument is not null || fullNameArgument is not null;
    }

    private bool IsValue(string arg)
    {
        return !string.IsNullOrWhiteSpace(arg) && !IsOption(arg, out _);
    }

    public bool IsValid => _isValid;
    
    public bool IsHelp => _options.Any(o => o.FullName.Equals("help", StringComparison.OrdinalIgnoreCase));
}