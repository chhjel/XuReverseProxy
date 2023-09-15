using Microsoft.Extensions.Logging;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Logging;

public class MemoryLogger : ILogger
{
    public static bool Enabled { get; set; }
    private static readonly List<LoggedEvent> _events = new();
    private static readonly object _eventsLock = new();
    private const int _maxCount = 1000;

    public virtual IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

    public virtual bool IsEnabled(LogLevel logLevel)
        => Enabled && logLevel > LogLevel.Debug;

    public virtual void Log<TState>(
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

        lock (_eventsLock)
        {
            _events.Add(new(DateTime.UtcNow, logLevel, eventId, exception, formatter(state, exception)));
            if (_events.Count > _maxCount)
            {
                _events.RemoveAt(0);
            }
        }
    }

    public static List<LoggedEvent> GetEvents()
    {
        lock (_events)
        {
            return _events.ToList();
        }
    }

    [GenerateFrontendModel]
    public record LoggedEvent(DateTime TimestampUtc, LogLevel LogLevel, EventId EventId, Exception? Exception, string Message);
}
