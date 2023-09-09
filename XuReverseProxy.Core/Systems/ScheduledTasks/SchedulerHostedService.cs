using Microsoft.Extensions.Logging;
using XuReverseProxy.Core.Systems.ScheduledTasks.Cron;

namespace XuReverseProxy.Core.Systems.ScheduledTasks;

public class SchedulerHostedService : HostedService
{
    public event EventHandler<UnobservedTaskExceptionEventArgs>? UnobservedTaskException;
    private readonly List<SchedulerTaskWrapper> _scheduledTasks = new();
    private readonly IEnumerable<IScheduledTask> _tasks;
    private readonly ILogger<SchedulerHostedService> _logger;

    public SchedulerHostedService(IEnumerable<IScheduledTask> scheduledTasks, ILogger<SchedulerHostedService> logger)
    {
        _tasks = scheduledTasks;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        InitTasks();
        while (!cancellationToken.IsCancellationRequested)
        {
            await ExecuteOnceAsync(cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }

    private void InitTasks()
    {
        DateTime referenceTime = DateTime.UtcNow;

        foreach (IScheduledTask scheduledTask in _tasks)
        {
            _scheduledTasks.Add(new(CrontabSchedule.Parse(scheduledTask.Schedule), scheduledTask, referenceTime));
        }
    }

    private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
    {
        TaskFactory taskFactory = new(TaskScheduler.Current);
        DateTime referenceTime = DateTime.UtcNow;

        List<SchedulerTaskWrapper> tasksThatShouldRun = _scheduledTasks.Where(t => t.ShouldRun(referenceTime)).ToList();

        foreach (SchedulerTaskWrapper? task in tasksThatShouldRun)
        {
            task.Increment();

            _ = await taskFactory.StartNew(
                async () =>
                {
                    try
                    {
                        await task.Task.ExecuteAsync(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Task '{task.Task.GetType().Name}' threw an exception.");
                        UnobservedTaskExceptionEventArgs args = new(ex as AggregateException ?? new AggregateException(ex));

                        UnobservedTaskException?.Invoke(this, args);

                        if (!args.Observed)
                        {
                            throw;
                        }
                    }
                },
                cancellationToken, TaskCreationOptions.None, TaskScheduler.Current).ConfigureAwait(false);
        }
    }

    private class SchedulerTaskWrapper
    {
        public CrontabSchedule Schedule { get; set; }
        public IScheduledTask Task { get; set; }
        public DateTime NextRunTime { get; set; }
        public DateTime LastRunTime { get; set; }

        public SchedulerTaskWrapper(CrontabSchedule schedule, IScheduledTask task, DateTime nextRunTime)
        {
            Schedule = schedule;
            Task = task;
            NextRunTime = nextRunTime;
        }

        public void Increment()
        {
            LastRunTime = NextRunTime;
            NextRunTime = Schedule.GetNextOccurrence(NextRunTime);
        }

        public bool ShouldRun(DateTime currentTime)
        {
            return NextRunTime < currentTime && LastRunTime != NextRunTime;
        }
    }
}
