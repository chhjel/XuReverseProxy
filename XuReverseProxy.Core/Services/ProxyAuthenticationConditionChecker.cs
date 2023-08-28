using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Services;

public interface IProxyAuthenticationConditionChecker
{
    bool ConditionPassed(ProxyAuthenticationCondition condition);
}

public class ProxyAuthenticationConditionChecker : IProxyAuthenticationConditionChecker
{
    public bool ConditionPassed(ProxyAuthenticationCondition condition)
        => condition.ConditionType switch
        {
            ProxyAuthenticationCondition.ProxyAuthenticationConditionType.DateTimeRange => CheckDateTimeRange(condition),
            ProxyAuthenticationCondition.ProxyAuthenticationConditionType.TimeRange => CheckTimeRange(condition),
            ProxyAuthenticationCondition.ProxyAuthenticationConditionType.WeekDays => CheckWeekdays(condition),
            _ => throw new NotImplementedException($"Condition type '{condition.ConditionType}' not implemented.")
        };

    private static bool CheckDateTimeRange(ProxyAuthenticationCondition condition)
    {
        if (condition.DateTimeUtc1 == null && condition.DateTimeUtc2 == null) return true;
        else if (condition.DateTimeUtc1 == null) return DateTime.UtcNow <= condition.DateTimeUtc2;
        else if (condition.DateTimeUtc2 == null) return DateTime.UtcNow >= condition.DateTimeUtc1;
        else return DateTime.UtcNow >= condition.DateTimeUtc1 && DateTime.UtcNow <= condition.DateTimeUtc2;
    }

    private static bool CheckTimeRange(ProxyAuthenticationCondition condition)
    {
        var now = TimeOnly.FromDateTime(DateTime.UtcNow.ToLocalTime());
        if (condition.TimeOnlyUtc1 == null && condition.TimeOnlyUtc2 == null) return true;
        else if (condition.TimeOnlyUtc1 == null) return now <= condition.TimeOnlyUtc2;
        else if (condition.TimeOnlyUtc2 == null) return now >= condition.TimeOnlyUtc1;
        // to < from | e.g. from 18:00 to 02:00
        else if (condition.TimeOnlyUtc2 < condition.TimeOnlyUtc1)
            return now >= condition.TimeOnlyUtc1 || now <= condition.TimeOnlyUtc2;
        // from < to | e.g. from 02:00 to 18:00
        else return now >= condition.TimeOnlyUtc1 && now <= condition.TimeOnlyUtc2;
    }

    private static bool CheckWeekdays(ProxyAuthenticationCondition condition)
    {
        if (condition.DaysOfWeekUtc == null || condition.DaysOfWeekUtc == ProxyAuthenticationCondition.DayOfWeekFlags.None) return true;

        var currentDayOfWeek = DateTime.UtcNow.DayOfWeek switch
        {
            DayOfWeek.Monday => ProxyAuthenticationCondition.DayOfWeekFlags.Monday,
            DayOfWeek.Tuesday => ProxyAuthenticationCondition.DayOfWeekFlags.Tuesday,
            DayOfWeek.Wednesday => ProxyAuthenticationCondition.DayOfWeekFlags.Wednesday,
            DayOfWeek.Thursday => ProxyAuthenticationCondition.DayOfWeekFlags.Thursday,
            DayOfWeek.Friday => ProxyAuthenticationCondition.DayOfWeekFlags.Friday,
            DayOfWeek.Saturday => ProxyAuthenticationCondition.DayOfWeekFlags.Saturday,
            DayOfWeek.Sunday => ProxyAuthenticationCondition.DayOfWeekFlags.Sunday,
            _ => ProxyAuthenticationCondition.DayOfWeekFlags.None
        };
        return condition.DaysOfWeekUtc?.HasFlag(currentDayOfWeek) == true;
    }
}
