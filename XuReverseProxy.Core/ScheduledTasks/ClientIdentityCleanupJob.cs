using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Systems.ScheduledTasks;

namespace XuReverseProxy.Core.ScheduledTasks;

public class ClientIdentityCleanupTask(IOptionsMonitor<ServerConfig> serverConfig, ILogger<ClientIdentityCleanupTask> logger, IServiceProvider serviceProvider) : IScheduledTask
{
#if DEBUG
    /// <summary>
    /// Every 1 minute.
    /// </summary>
    public string Schedule { get; } = "0 */1 * * * *";
#else
    /// <summary>
    /// Every 30 minutes.
    /// </summary>
    public string Schedule { get; } = "0 */30 * * * *";
#endif

    public string Name { get; } = "Client Identity Maintenance Job";

    public string Description { get; } = "Removes old client identities.";

    public async Task<ScheduledTaskResult> ExecuteAsync(ScheduledTaskStatus status, CancellationToken cancellationToken)
    {
        var result = new ScheduledTaskResult() { JobType = GetType(), StartedAtUtc = DateTime.UtcNow, Result = string.Empty };
        logger.LogInformation($"Starting ClientIdentityCleanupTask.");

        var config = serverConfig.CurrentValue.Jobs.ClientIdentityCleanupJob;
        if (config?.Enabled != true) return result.SetResult("Job not enabled");

        using var scope = serviceProvider.CreateScope();
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
            result.Result += $"Deleted not accessed: {deletedCount}. ";
            if (deletedCount > 0)
                logger.LogInformation("RemoveIfNotAccessedInMinutes.Deleted = {deletedCount}", deletedCount);
        }

        if (config.RemoveIfNotAttemptedAccessedInMinutes > 0)
        {
            status.Message = "Cleaning clients not attempted accessed in some time.";
            var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"ProxyClientIdentities\" " +
                $"WHERE (timezone('utc', now()) - \"{nameof(ProxyClientIdentity.LastAttemptedAccessedAtUtc)}\") > '{config.RemoveIfNotAttemptedAccessedInMinutes.Value} minute'::interval;",
                cancellationToken);

            totalDeleted += deletedCount;
            result.Result += $"Deleted not attempted accessed: {deletedCount}. ";
            if (deletedCount > 0)
                logger.LogInformation("RemoveIfNotAttemptedAccessedInMinutes.Deleted = {deletedCount}", deletedCount);
        }

        if (config.RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes > 0)
        {
            status.Message = "Cleaning clients never accessed and not attempted accessed in some time.";
            var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"ProxyClientIdentities\" " +
                $"WHERE \"{nameof(ProxyClientIdentity.LastAccessedAtUtc)}\" is NULL AND (timezone('utc', now()) - \"{nameof(ProxyClientIdentity.LastAttemptedAccessedAtUtc)}\") > '{config.RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes.Value} minute'::interval;",
                cancellationToken);

            totalDeleted += deletedCount;
            result.Result += $"Deleted never accessed and not attempted accessed in some time: {deletedCount}. ";
            if (deletedCount > 0)
                logger.LogInformation("RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes.Deleted = {deletedCount}", deletedCount);
        }

        status.Message = $"Done. Total deleted: {totalDeleted}.";
        result.Success = true;
        return result;
    }
}
