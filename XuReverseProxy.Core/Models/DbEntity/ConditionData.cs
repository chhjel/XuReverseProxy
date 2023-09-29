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
    public ConditionType Type { get; set; }
    public bool Inverted { get; set; }

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
                ? (!Inverted ? "Any day" : "No days")
                : ($"{(!Inverted ? string.Empty : "Not ")}" + TKEnumUtils.TryGetEnumFlaggedValueNames(DaysOfWeekUtc.Value).JoinForSentence()),
            ConditionType.IPEquals => !Inverted ? $"IP equals '{IPCondition}'" : $"IP does not equal '{IPCondition}'",
            ConditionType.IPRegex => !Inverted ? $"IP matches regex pattern '{IPCondition}'" : $"IP does not match regex pattern '{IPCondition}'",
            ConditionType.IPCIDRRange => !Inverted ? $"IP matches CIDR range '{IPCondition}'" : $"IP does not match CIDR range '{IPCondition}'",
            ConditionType.IsLocalRequest => !Inverted ? "Request is local" : "Request is not local",
            _ => "Unknown"
        };

    private string CreateRangeText<T>(T? from, T? to, Func<T, string> formatter)
    {
        string prefix(string upper)
            => Inverted ? $"Not {upper.ToLower()}" : upper;

        if (from == null && to == null) return string.Empty;
        else if (from == null) return $"{prefix("Before")} {formatter(to!)}.";
        else if (to == null) return $"{prefix("After")} {formatter(from!)}.";
        else return $"{prefix("Between")} {formatter(from!)} and {formatter(to!)}.";
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
