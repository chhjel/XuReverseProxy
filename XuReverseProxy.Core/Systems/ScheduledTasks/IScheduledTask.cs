namespace XuReverseProxy.Core.Systems.ScheduledTasks;

public interface IScheduledTask
{
    string Schedule { get; }
    Task<ScheduledTaskResult> ExecuteAsync(CancellationToken cancellationToken, ScheduledTaskStatus status);
}