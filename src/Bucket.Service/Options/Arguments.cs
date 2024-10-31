using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ardalis.GuardClauses;

namespace Bucket.Service.Options;

public sealed class Arguments
{
    private readonly string[] _args;
    private readonly List<Argument> _arguments = new();

    public Arguments(string args)
    {
        var arguments = args.Split(' ');
        var trimmed = arguments.Select(s => s.Trim());
        
        _args = trimmed.ToArray();
    }

    public Arguments(string[] args)
    {
        _args = args;
    }

    public Arguments AddArgument(string shortName, string fullName, string description)
    {
        Guard.Against.NullOrWhiteSpace(shortName);
        Guard.Against.NullOrWhiteSpace(fullName);
        Guard.Against.NullOrWhiteSpace(description);
        
        var argument = new Argument(shortName, fullName, description);
        
        return AddArgument(argument);
    }

    public Arguments AddArgument(Argument argument)
    {
        _arguments.Add(argument);    
        
        return this;
    }

    public IReadOnlyCollection<Argument> GetOptions()
    {
        return Array.Empty<Argument>();
    }

    public string GetOptionValue(string shortName)
    {
        Guard.Against.NullOrWhiteSpace(shortName);
        
        return string.Empty;
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
}