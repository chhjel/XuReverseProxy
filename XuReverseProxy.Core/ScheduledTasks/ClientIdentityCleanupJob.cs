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
#if DEBUG
    /// <summary>
    /// Every 1 minute.
    /// </summary>
    public string Schedule => "0 */1 * * * *";
#else
    /// <summary>
    /// Every 30 minutes.
    /// </summary>
    public string Schedule => "0 */30 * * * *";
#endif

    private readonly IOptionsMonitor<ServerConfig> _serverConfig;
    private readonly ILogger<ClientIdentityCleanupTask> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ClientIdentityCleanupTask(IOptionsMonitor<ServerConfig> serverConfig, ILogger<ClientIdentityCleanupTask> logger, IServiceProvider serviceProvider)
    {
        _serverConfig = serverConfig;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<ScheduledTaskResult> ExecuteAsync(CancellationToken cancellationToken, ScheduledTaskStatus status)
    {
        var result = new ScheduledTaskResult() { JobType = GetType(), StartedAtUtc = DateTime.UtcNow, Result = string.Empty };
        _logger.LogInformation($"Starting ClientIdentityCleanupTask.");

        var config = _serverConfig.CurrentValue.Jobs.ClientIdentityCleanupJob;
        if (config?.Enabled != true) return result.SetResult("Job not enabled");

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var totalDeleted = 0;

        if (config.RemoveIfNotAccessedInMinutes > 0)
        {
            status.Message = "Cleaning clients not accessed in some time.";
            var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"ProxyClientIdentities\" " +
                $"WHERE (timezone('utc', now()) - \"{nameof(ProxyClientIdentity.LastAccessedAtUtc)}\") > '{config.RemoveIfNotAccessedInMinutes.Value} minute'::interval;",
                cancellationToken);

            totalDeleted += deletedCount;
            result.Result += $"RemoveIfNotAccessedInMinutes.Deleted = {deletedCount}. ";
            if (deletedCount > 0)
                _logger.LogInformation($"RemoveIfNotAccessedInMinutes.Deleted = {deletedCount}");
        }

        if (config.RemoveIfNotAttemptedAccessedInMinutes > 0)
        {
            status.Message = "Cleaning clients not attempted accessed in some time.";
            var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"ProxyClientIdentities\" " +
                $"WHERE (timezone('utc', now()) - \"{nameof(ProxyClientIdentity.LastAttemptedAccessedAtUtc)}\") > '{config.RemoveIfNotAttemptedAccessedInMinutes.Value} minute'::interval;",
                cancellationToken);

            totalDeleted += deletedCount;
            result.Result += $"RemoveIfNotAttemptedAccessedInMinutes.Deleted = {deletedCount}. ";
            if (deletedCount > 0)
                _logger.LogInformation($"RemoveIfNotAttemptedAccessedInMinutes.Deleted = {deletedCount}");
        }

        if (config.RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes > 0)
        {
            status.Message = "Cleaning clients never accessed and not attempted accessed in some time.";
            var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"ProxyClientIdentities\" " +
                $"WHERE \"{nameof(ProxyClientIdentity.LastAccessedAtUtc)}\" is NULL AND (timezone('utc', now()) - \"{nameof(ProxyClientIdentity.LastAttemptedAccessedAtUtc)}\") > '{config.RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes.Value} minute'::interval;",
                cancellationToken);

            totalDeleted += deletedCount;
            result.Result += $"RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes.Deleted = {deletedCount}. ";
            if (deletedCount > 0)
                _logger.LogInformation($"RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes.Deleted = {deletedCount}");
        }

        status.Message = $"Done. Total deleted: {totalDeleted}.";
        result.Success = true;
        return result;
    }
}
