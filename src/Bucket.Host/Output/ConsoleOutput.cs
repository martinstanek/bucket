using System;
using Bucket.Service.Services;

namespace Bucket.Host.Output;

public sealed class ConsoleOutput : IOutput
{
    public void WriteLine(string text)
    {
        Console.WriteLine(text);
    }
}