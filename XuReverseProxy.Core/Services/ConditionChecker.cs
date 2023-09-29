using Microsoft.AspNetCore.Http;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Web.Core.Utils;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Models.Enums;
using static XuReverseProxy.Core.Models.DbEntity.ConditionData;

namespace XuReverseProxy.Core.Services;

public interface IConditionChecker
{
    bool ConditionsPassed(IEnumerable<ConditionData> conditions, ConditionContext context);
    bool ConditionPassed(ConditionData condition, ConditionContext context);
    ConditionContext CreateContext();
}

public class ConditionContext
{
    public string? RequestIP { get; set; }
    public HttpContext? HttpContext { get; set; }
}

public class ConditionChecker : IConditionChecker
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ConditionChecker(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public ConditionContext CreateContext()
    {
        var httpContext = _contextAccessor.HttpContext;
        return new()
        {
            HttpContext = httpContext,
            RequestIP = httpContext == null ? null : TKRequestUtils.GetIPAddress(httpContext),
        };
    }

    public bool ConditionsPassed(IEnumerable<ConditionData> conditions, ConditionContext context)
    {
        if (conditions?.Any() != true) return true;

        var groups = conditions.GroupBy(x => x.Group).OrderBy(x => x.Key);
        foreach (var group in groups)
        {
            if (AllConditionsPassed(group, context)) return true;
        }

        return false;
    }

    private bool AllConditionsPassed(IEnumerable<ConditionData> conditions, ConditionContext context)
    {
        if (conditions?.Any() != true) return false;

        foreach (var condition in conditions)
        {
            if (!ConditionPassed(condition, context)) return false;
        }

        return true;
    }

    public bool ConditionPassed(ConditionData condition, ConditionContext context)
    {
        try
        {
            var result = condition.Type switch
            {
                ConditionType.DateTimeRange => CheckDateTimeRange(condition),
                ConditionType.TimeRange => CheckTimeRange(condition),
                ConditionType.WeekDays => CheckWeekdays(condition),
                ConditionType.IPEquals => CheckIPEquals(condition, context),
                ConditionType.IPRegex => CheckIPRegex(condition, context),
                ConditionType.IPCIDRRange => CheckIPCidrRange(condition, context),
                ConditionType.IsLocalRequest => TKRequestUtils.IsLocalRequest(context.HttpContext),
                _ => throw new NotImplementedException($"Condition type '{condition.Type}' not implemented.")
            };
            if (condition.Inverted) result = !result;
            return result;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static bool CheckIPEquals(ConditionData condition, ConditionContext context)
        => condition.IPCondition?.Equals(context?.RequestIP, StringComparison.OrdinalIgnoreCase) == true;

    private bool CheckIPRegex(ConditionData condition, ConditionContext context)
        => !string.IsNullOrWhiteSpace(condition.IPCondition)
        && !string.IsNullOrWhiteSpace(context.RequestIP)
        && TryRegexMatch(condition.IPCondition, context.RequestIP);

    private static bool CheckIPCidrRange(ConditionData condition, ConditionContext context)
        => !string.IsNullOrWhiteSpace(condition.IPCondition)
        && !string.IsNullOrWhiteSpace(context.RequestIP)
        && context.RequestIP?.Equals("localhost", StringComparison.OrdinalIgnoreCase) == false
        && TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(context.RequestIP, condition.IPCondition);

    private static readonly TKCachedRegexContainer _regexCache = new();
    private static bool TryRegexMatch(string pattern, string value)
    {
        try
        {
            var regex = _regexCache.GetRegex(pattern, false);
            return regex.IsMatch(value);
        }
        catch (Exception) { return false; }
    }

    private static bool CheckDateTimeRange(ConditionData condition)
    {
        if (condition.DateTimeUtc1 == null && condition.DateTimeUtc2 == null) return true;
        else if (condition.DateTimeUtc1 == null) return DateTime.UtcNow <= condition.DateTimeUtc2;
        else if (condition.DateTimeUtc2 == null) return DateTime.UtcNow >= condition.DateTimeUtc1;
        else return DateTime.UtcNow >= condition.DateTimeUtc1 && DateTime.UtcNow <= condition.DateTimeUtc2;
    }

    private static bool CheckTimeRange(ConditionData condition)
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

    private static bool CheckWeekdays(ConditionData condition)
    {
        if (condition.DaysOfWeekUtc == null || condition.DaysOfWeekUtc == DayOfWeekFlags.None) return true;

        var currentDayOfWeek = DateTime.UtcNow.DayOfWeek switch
        {
            DayOfWeek.Monday => DayOfWeekFlags.Monday,
            DayOfWeek.Tuesday => DayOfWeekFlags.Tuesday,
            DayOfWeek.Wednesday => DayOfWeekFlags.Wednesday,
            DayOfWeek.Thursday => DayOfWeekFlags.Thursday,
            DayOfWeek.Friday => DayOfWeekFlags.Friday,
            DayOfWeek.Saturday => DayOfWeekFlags.Saturday,
            DayOfWeek.Sunday => DayOfWeekFlags.Sunday,
            _ => DayOfWeekFlags.None
        };
        return condition.DaysOfWeekUtc?.HasFlag(currentDayOfWeek) == true;
    }
}
