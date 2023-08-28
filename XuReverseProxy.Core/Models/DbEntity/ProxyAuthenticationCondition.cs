using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Util;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

public class ProxyAuthenticationCondition
{
    public Guid Id { get; set; }
    public Guid AuthenticationDataId { get; set; }
    public ProxyAuthenticationData AuthenticationData { get; set; } = null!;
    public ProxyAuthenticationConditionType ConditionType { get; set; }

    public DateTime? DateTimeUtc1 { get; set; }
    public DateTime? DateTimeUtc2 { get; set; }
    public TimeOnly? TimeOnlyUtc1 { get; set; }
    public TimeOnly? TimeOnlyUtc2 { get; set; }
    public DayOfWeekFlags? DaysOfWeekUtc { get; set; }

    public string CreateSummary()
        => ConditionType switch
        {
            ProxyAuthenticationConditionType.DateTimeRange => CreateRangeText(DateTimeUtc1, DateTimeUtc2, x => $"{x!.Value.ToLocalTime():dd/MM HH:mm}"),
            ProxyAuthenticationConditionType.TimeRange => CreateRangeText(TimeOnlyUtc1, TimeOnlyUtc2, x => $"{x}"),
            ProxyAuthenticationConditionType.WeekDays => (DaysOfWeekUtc == null || DaysOfWeekUtc == DayOfWeekFlags.None) 
                ? "Any day"
                : TKEnumUtils.TryGetEnumFlaggedValueNames(DaysOfWeekUtc.Value).JoinForSentence(),
            _ => "Unknown"
        };

    private static string CreateRangeText<T>(T? from, T? to, Func<T, string> formatter)
    {
        if (from == null && to == null) return string.Empty;
        else if (from == null) return $"Before {formatter(to!)}.";
        else if (to == null) return $"After {formatter(from!)}.";
        else return $"Between {formatter(from!)} and {formatter(to!)}.";
    }

    [Flags]
    public enum DayOfWeekFlags
    {
        None,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64
    }

    [GenerateFrontendModel]
    public enum ProxyAuthenticationConditionType
    {
        /// <summary>
        /// <see cref="DateTimeUtc1"/> - <see cref="DateTimeUtc2"/>
        /// </summary>
        DateTimeRange,

        /// <summary>
        /// <see cref="TimeOnlyUtc1"/> - <see cref="TimeOnlyUtc2"/>
        /// </summary>
        TimeRange,

        /// <summary>
        /// <see cref="DaysOfWeekUtc"/>
        /// </summary>
        WeekDays
    }
}
