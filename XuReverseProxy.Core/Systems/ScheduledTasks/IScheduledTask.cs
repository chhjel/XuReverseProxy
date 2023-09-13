namespace XuReverseProxy.Core.Systems.ScheduledTasks;

public interface IScheduledTask
{
    string Name { get; }
    string Description { get; }
    string Schedule { get; }
    Task<ScheduledTaskResult> ExecuteAsync(CancellationToken cancellationToken, ScheduledTaskStatus status);
}