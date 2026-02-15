using System;
using System.Collections.Generic;
using System.Text;

namespace PoolController;

internal static class Log
{
    public static void LogMessage(string message)
    {
        if (Console.IsOutputRedirected)
            return;
        Console.WriteLine(message);
    }

    public static void LogWarning(string message)
    {
        if (Console.IsOutputRedirected)
            return;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Error.WriteLine(message);
        Console.ResetColor();
    }
    public static void LogError(string message)
    {
        if (Console.IsOutputRedirected)
            return;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(message);
        Console.ResetColor();
    }
}
