using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using XuReverseProxy.Core.Systems.ScheduledTasks.Cron;

namespace XuReverseProxy.Core.Systems.ScheduledTasks;

public class SchedulerHostedService : HostedService
{
    public event EventHandler<UnobservedTaskExceptionEventArgs>? UnobservedTaskException;
    private readonly List<SchedulerTaskWrapper> _scheduledTasks = new();
    private readonly IEnumerable<IScheduledTask> _tasks;
    private readonly ILogger<SchedulerHostedService> _logger;

    public readonly ConcurrentDictionary<Type, ScheduledTaskStatus> Statuses = new();
    public readonly ConcurrentDictionary<Type, ScheduledTaskResult> LatestResults = new();

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
            var taskType = scheduledTask.GetType();
            if (!Statuses.ContainsKey(taskType)) Statuses[taskType] = new() { JobType = taskType, Message = "Not started yet." };

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
                    var taskType = task.Task.GetType();
                    var startedAt = DateTime.UtcNow;

                    var status = Statuses[taskType];
                    status.LastStartedAt = startedAt;
                    status.StoppedAt = null;
                    status.Message = "Started";
                    status.IsRunning = true;
                    status.Failed = false;

                    try
                    {
                        var result = await task.Task.ExecuteAsync(status, cancellationToken);
                        result.StoppedAtUtc = DateTime.UtcNow;

                        LatestResults[taskType] = result;
                    }
                    catch (Exception ex)
                    {
                        status.Failed = true;
                        LatestResults[taskType] = new()
                        {
                            JobType = taskType,
                            Success = false,
                            Result = $"Exception was thrown during job execution.",
                            Exception = ex,
                            StartedAtUtc = startedAt,
                            StoppedAtUtc = DateTime.UtcNow
                        };

                        var taskName = task.Task.GetType().Name;
                        _logger.LogError(ex, "Task '{taskName}' threw an exception.", taskName);
                        UnobservedTaskExceptionEventArgs args = new(ex as AggregateException ?? new AggregateException(ex));

                        UnobservedTaskException?.Invoke(this, args);

                        if (!args.Observed)
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        status.IsRunning = false;
                        status.StoppedAt = DateTime.UtcNow;
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
