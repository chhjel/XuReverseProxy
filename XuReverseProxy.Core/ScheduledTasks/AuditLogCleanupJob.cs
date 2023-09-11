using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Systems.ScheduledTasks;

namespace XuReverseProxy.Core.ScheduledTasks;

public class AuditLogsCleanupJob : IScheduledTask
{
#if DEBUG
    /// <summary>
    /// Every 1 minute.
    /// </summary>
    public string Schedule => "0 */1 * * * *";
#else
    /// <summary>
    /// Every 60 minutes.
    /// </summary>
    public string Schedule => "0 */60 * * * *";
#endif

    private readonly IOptionsMonitor<ServerConfig> _serverConfig;
    private readonly ILogger<AuditLogsCleanupJob> _logger;
    private readonly IServiceProvider _serviceProvider;

    public AuditLogsCleanupJob(IOptionsMonitor<ServerConfig> serverConfig, ILogger<AuditLogsCleanupJob> logger, IServiceProvider serviceProvider)
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
        //var totalDeleted = 0;

        // todo

        //result.Result += $"Deleted never accessed and not attempted accessed in some time: {deletedCount}. ";

        //status.Message = $"Done. Total deleted: {totalDeleted}.";
        result.Success = true;
        return result;
    }
}
