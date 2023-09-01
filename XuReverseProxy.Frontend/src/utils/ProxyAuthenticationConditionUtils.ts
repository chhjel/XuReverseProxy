import { ProxyAuthenticationConditionType } from "@generated/Enums/Core/ProxyAuthenticationConditionType";
import { ProxyAuthenticationCondition } from "@generated/Models/Core/ProxyAuthenticationCondition";

export function createProxyAuthenticationConditionSummary(condition: ProxyAuthenticationCondition): string {
    if (condition.conditionType == ProxyAuthenticationConditionType.DateTimeRange) {
        if (!condition.dateTimeUtc1 && !condition.dateTimeUtc2) return condition.conditionType;
        else if (!condition.dateTimeUtc1) return `Before ${formatDate(condition.dateTimeUtc2)}`;
        else if (!condition.dateTimeUtc2) return `After ${formatDate(condition.dateTimeUtc1)}`;
        return `Between ${formatDate(condition.dateTimeUtc1)} and ${formatDate(condition.dateTimeUtc2)}`;
    }
    else if (condition.conditionType == ProxyAuthenticationConditionType.TimeRange) {
        if (!condition.timeOnlyUtc1 && !condition.timeOnlyUtc2) return condition.conditionType;
        else if (!condition.timeOnlyUtc1) return `Before ${formatTime(condition.timeOnlyUtc2)}`;
        else if (!condition.timeOnlyUtc2) return `After ${formatTime(condition.timeOnlyUtc1)}`;
        return `Between ${formatTime(condition.timeOnlyUtc1)} and ${formatTime(condition.timeOnlyUtc2)}`;
    }
    else if (condition.conditionType == ProxyAuthenticationConditionType.WeekDays) {
        return (!condition.daysOfWeekUtc) ? 'Weekdays: None' : `${condition.daysOfWeekUtc}`;
    }
    else return condition.conditionType;
}

function formatDate(raw: Date | string): string {
    if (raw == null) return '';
    let date: Date = (typeof raw === 'string') ? new Date(raw) : raw;
    return date.toLocaleString();
}

function formatTime(raw: Date | string): string {
    return raw?.toString();
}
