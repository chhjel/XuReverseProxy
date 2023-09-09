namespace XuReverseProxy.Core.Systems.ScheduledTasks;

public interface IScheduledTask
{
    string Schedule { get; }
    Task ExecuteAsync(CancellationToken cancellationToken);
}