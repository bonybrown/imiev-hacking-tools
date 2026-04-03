using Microsoft.Extensions.Logging;

sealed class StdoutLogger(LogLevel minimumLevel) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= minimumLevel;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message) && exception is null)
        {
            return;
        }

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{logLevel}] {message}");
        if (exception is not null)
        {
            Console.WriteLine(exception);
        }
    }
}
