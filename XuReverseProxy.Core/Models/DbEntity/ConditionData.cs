using QoDL.Toolkit.Core.Extensions;
using QoDL.Toolkit.Core.Util;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.Enums;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class ConditionData : IHasId
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }

    /// <summary>
    /// Conditions within the same group will be AND'ed, results of each group will be OR'ed.
    /// </summary>
    public int Group { get; set; }

    // E.g:
    // { Monday-Friday && 8-16 }
    // ||
    // { CIDR range }
    // ||
    // { LAN request }
    // ||
    // { Saturday && IP matches }

    public ConditionType Type { get; set; }

    public DateTime? DateTimeUtc1 { get; set; }
    public DateTime? DateTimeUtc2 { get; set; }
    public TimeOnly? TimeOnlyUtc1 { get; set; }
    public TimeOnly? TimeOnlyUtc2 { get; set; }
    public DayOfWeekFlags? DaysOfWeekUtc { get; set; }
    public string? IPCondition { get; set; }

    public string CreateSummary()
        => Type switch
        {
            ConditionType.DateTimeRange => CreateRangeText(DateTimeUtc1, DateTimeUtc2, x => $"{x!.Value.ToLocalTime():dd/MM HH:mm}"),
            ConditionType.TimeRange => CreateRangeText(TimeOnlyUtc1, TimeOnlyUtc2, x => $"{x}"),
            ConditionType.WeekDays => (DaysOfWeekUtc == null || DaysOfWeekUtc == DayOfWeekFlags.None)
                ? "Any day"
                : TKEnumUtils.TryGetEnumFlaggedValueNames(DaysOfWeekUtc.Value).JoinForSentence(),
            ConditionType.IPEquals => $"IP equals '{IPCondition}'",
            ConditionType.IPRegex => $"IP matches regex pattern '{IPCondition}'",
            ConditionType.IPCIDRRange => $"IP matches CIDR range '{IPCondition}'",
            ConditionType.IsLocalRequest => "Request is local",
            _ => "Unknown"
        };

    private static string CreateRangeText<T>(T? from, T? to, Func<T, string> formatter)
    {
        if (from == null && to == null) return string.Empty;
        else if (from == null) return $"Before {formatter(to!)}.";
        else if (to == null) return $"After {formatter(from!)}.";
        else return $"Between {formatter(from!)} and {formatter(to!)}.";
    }

    [GenerateFrontendModel]
    public enum ConditionType
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
        WeekDays,

        /// <summary>
        /// IP matches <see cref="IPCondition"/>.
        /// </summary>
        IPEquals,

        /// <summary>
        /// IP matches RegEx <see cref="IPCondition"/>.
        /// </summary>
        IPRegex,

        /// <summary>
        /// IP matches CIDR range <see cref="IPCondition"/>.
        /// </summary>
        IPCIDRRange,

        /// <summary>
        /// Request is local.
        /// </summary>
        IsLocalRequest,
    }
}
