namespace XuReverseProxy.Core.Systems.ScheduledTasks;

public interface IScheduledTask
{
    string Name { get; }
    string Description { get; }
    string Schedule { get; }
    Task<ScheduledTaskResult> ExecuteAsync(ScheduledTaskStatus status, CancellationToken cancellationToken);
}