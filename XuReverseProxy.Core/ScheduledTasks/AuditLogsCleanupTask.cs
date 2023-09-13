using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Systems.ScheduledTasks;

namespace XuReverseProxy.Core.ScheduledTasks;

public class AuditLogsCleanupTask : IScheduledTask
{
#if DEBUG
    /// <summary>
    /// Every 1 minute.
    /// </summary>
    public string Schedule { get; } = "0 */1 * * * *";
#else
    /// <summary>
    /// Every 60 minutes.
    /// </summary>
    public string Schedule { get; } = "0 */60 * * * *";
#endif

    public string Name { get; } = "Audit Log Maintenance Job";

    public string Description { get; } = "Removes old audit log entries.";

    private readonly IOptionsMonitor<ServerConfig> _serverConfig;
    private readonly ILogger<AuditLogsCleanupTask> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AuditLogsCleanupTask(IOptionsMonitor<ServerConfig> serverConfig, ILogger<AuditLogsCleanupTask> logger, IServiceProvider serviceProvider)
    {
        _serverConfig = serverConfig;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<ScheduledTaskResult> ExecuteAsync(CancellationToken cancellationToken, ScheduledTaskStatus status)
    {
        var result = new ScheduledTaskResult() { JobType = GetType(), StartedAtUtc = DateTime.UtcNow, Result = string.Empty };
        _logger.LogInformation($"Starting AuditLogsCleanupJob.");

        var config = _serverConfig.CurrentValue.Jobs.AuditLogCleanupJob;
        if (config?.Enabled != true) return result.SetResult("Job not enabled");

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var totalDeleted = 0;

        if (config.MaxAdminEntryAgeInHours > 0)
        {
            status.Message = "Cleaning admin events.";
            var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"AdminAuditLogEntries\" " +
                $"WHERE (timezone('utc', now()) - \"{nameof(AdminAuditLogEntry.TimestampUtc)}\") > '{config.MaxAdminEntryAgeInHours.Value} hour'::interval;",
                cancellationToken);

            totalDeleted += deletedCount;
            result.Result += $"Deleted admin events: {deletedCount}. ";
            if (deletedCount > 0)
                _logger.LogInformation($"Deleted admin events = {deletedCount}");
        }

        if (config.MaxClientEntryAgeInHours > 0)
        {
            status.Message = "Cleaning client events.";
            var deletedCount = await dbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM \"ClientAuditLogEntries\" " +
                $"WHERE (timezone('utc', now()) - \"{nameof(ClientAuditLogEntry.TimestampUtc)}\") > '{config.MaxClientEntryAgeInHours.Value} hour'::interval;",
                cancellationToken);

            totalDeleted += deletedCount;
            result.Result += $"Deleted client events: {deletedCount}. ";
            if (deletedCount > 0)
                _logger.LogInformation($"Deleted client events = {deletedCount}");
        }

        status.Message = $"Done. Total deleted: {totalDeleted}.";
        result.Success = true;
        return result;
    }
}
