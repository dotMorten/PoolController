using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PoolController;

internal static class Log
{
    public class LoggerProvider : ILoggerProvider
    {
        private class Logger : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return logLevel > LogLevel.Information;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (exception is not null)
                {
                    if (logLevel == LogLevel.Warning)
                        PoolController.Log.LogWarning(formatter(state, exception));
                    else if (logLevel == LogLevel.Error)
                        PoolController.Log.LogError($"{formatter(state, exception)}\n{exception.StackTrace}");
                }
            }
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new Logger();
        }

        public void Dispose()
        {
        }
    }

    public static void PurgeLogFile()
    {
        lock (logfileLock)
        {
            if (File.Exists(LogFileName))
            {
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".PoolControllerLogs");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                File.Move(LogFileName, Path.Combine(folder, DateTime.Now.ToString("s", CultureInfo.InvariantCulture).Replace(":", "") + ".log"));
            }
        }
    }

    public static void LogMessage(string message)
    {
        if (Console.IsOutputRedirected)
            return;
        Console.WriteLine(message);
        WriteToLog(message);
    }

    public static void LogWarning(string message)
    {
        if (Console.IsOutputRedirected)
            return;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Error.WriteLine(message);
        Console.ResetColor();
        WriteToLog("WARNING: " + message);
    }

    public static void LogError(string message)
    {
        if (Console.IsOutputRedirected)
            return;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(message);
        Console.ResetColor();
        WriteToLog("ERROR: " + message);
    }

    static object logfileLock = new object();

    private static void WriteToLog(string text)
    {
        lock (logfileLock)
        {
            File.WriteAllText(LogFileName, $"{DateTime.Now.ToString("s", CultureInfo.InvariantCulture)}: {text}\n");
        }
    }

    public static string LogFileName
    {
        get
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "poolcontroller.log");
        }
    }
}
