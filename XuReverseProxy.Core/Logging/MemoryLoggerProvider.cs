using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace XuReverseProxy.Core.Logging;

[ProviderAlias("MemoryLogger")]
public sealed class MemoryLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, MemoryLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);


    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(categoryName, name => new());

    public void Dispose() => _loggers.Clear();
}
