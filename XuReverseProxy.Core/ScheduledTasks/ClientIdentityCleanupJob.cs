using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Systems.ScheduledTasks;

namespace XuReverseProxy.Core.ScheduledTasks;

public class ClientIdentityCleanupTask : IScheduledTask
{
    /// <summary>
    /// Every 30 minutes.
    /// </summary>
    public string Schedule => "0 */30 * * * *";

    private readonly IOptionsMonitor<ServerConfig> _serverConfig;
    private readonly ILogger<ClientIdentityCleanupTask> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ClientIdentityCleanupTask(IOptionsMonitor<ServerConfig> serverConfig, ILogger<ClientIdentityCleanupTask> logger, IServiceProvider serviceProvider)
    {
        _serverConfig = serverConfig;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting ClientIdentityCleanupTask.");

        var config = _serverConfig.CurrentValue.Jobs.ClientIdentityCleanupJob;
        if (config?.Enabled != true) return;

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (config.RemoveIfNotAccessedInMinutes > 0)
        {
            var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"ProxyClientIdentities\" " +
                $"WHERE (timezone('utc', now()) - \"{nameof(ProxyClientIdentity.LastAccessedAtUtc)}\") > '{config.RemoveIfNotAccessedInMinutes.Value} minute'::interval;",
                cancellationToken);
            if (deletedCount > 0)
                _logger.LogInformation($"RemoveIfNotAccessedInMinutes.Deleted = {deletedCount}");
        }

        if (config.RemoveIfNotAttemptedAccessedInMinutes > 0)
        {
            var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"ProxyClientIdentities\" " +
                $"WHERE (timezone('utc', now()) - \"{nameof(ProxyClientIdentity.LastAttemptedAccessedAtUtc)}\") > '{config.RemoveIfNotAttemptedAccessedInMinutes.Value} minute'::interval;",
                cancellationToken);
            if (deletedCount > 0)
                _logger.LogInformation($"RemoveIfNotAttemptedAccessedInMinutes.Deleted = {deletedCount}");
        }

        if (config.RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes > 0)
        {
            var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"ProxyClientIdentities\" " +
                $"WHERE \"{nameof(ProxyClientIdentity.LastAccessedAtUtc)}\" is NULL AND (timezone('utc', now()) - \"{nameof(ProxyClientIdentity.LastAttemptedAccessedAtUtc)}\") > '{config.RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes.Value} minute'::interval;",
                cancellationToken);
            if (deletedCount > 0)
                _logger.LogInformation($"RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes.Deleted = {deletedCount}");
        }
    }
}
