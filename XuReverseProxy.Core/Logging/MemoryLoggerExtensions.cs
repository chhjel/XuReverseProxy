using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace XuReverseProxy.Core.Logging;

public static class MemoryLoggerExtensions
{
    public static ILoggingBuilder AddMemoryLogger(this ILoggingBuilder builder)
    {
        builder.AddConfiguration();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, MemoryLoggerProvider>());
        return builder;
    }
}