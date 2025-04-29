using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

public class FileLogger : ILogger
{
    private readonly string _name;
    private readonly FileLoggerOptions _options;
    private static readonly object _lock = new();

    public FileLogger(string name, FileLoggerOptions options)
    {
        _name = name;
        _options = options;
    }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) =>
        logLevel >= _options.MinLevel;

    public void Log<TState>(LogLevel logLevel,
                            EventId eventId,
                            TState state,
                            Exception? exception,
                            Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = new StringBuilder();
        message.Append($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} ");
        message.Append($"[{logLevel}] ");
        message.Append($"({_name}) ");
        message.Append(formatter(state, exception));
        if (exception is not null)
            message.Append($" {exception}");

        var logLine = message.ToString();

        lock (_lock)
        {
            Directory.CreateDirectory(_options.LogDirectory);
            var filePath = Path.Combine(_options.LogDirectory, _options.FileName);
            File.AppendAllText(filePath, logLine + Environment.NewLine);
        }
    }
}

public class FileLoggerProvider : ILoggerProvider
{
    private readonly FileLoggerOptions _options;

    public FileLoggerProvider(FileLoggerOptions options)
    {
        _options = options;
    }

    public ILogger CreateLogger(string categoryName) =>
        new FileLogger(categoryName, _options);

    public void Dispose() { /* nothing to dispose */ }
}

public class FileLoggerOptions
{
    /// <summary>Where to write logs; will be created if missing.</summary>
    public string LogDirectory { get; set; } = "logs";

    /// <summary>Filename (e.g. "trace.log" or include date tokens if you like).</summary>
    public string FileName { get; set; } = "app.log";

    /// <summary>Minimum level to write.</summary>
    public LogLevel MinLevel { get; set; } = LogLevel.Information;
}
