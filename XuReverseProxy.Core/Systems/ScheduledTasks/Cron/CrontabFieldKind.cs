namespace XuReverseProxy.Core.Systems.ScheduledTasks.Cron;

[Serializable]
public enum CrontabFieldKind
{
    Second,
    Minute,
    Hour,
    Day,
    Month,
    DayOfWeek
}