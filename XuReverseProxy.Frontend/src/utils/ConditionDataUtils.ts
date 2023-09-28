import { ConditionType } from "@generated/Enums/Core/ConditionType";
import { ConditionData } from "@generated/Models/Core/ConditionData";
import DateFormats from "./DateFormats";

export function createConditionDataSummary(condition: ConditionData): string {
  if (condition.type == ConditionType.DateTimeRange) {
    if (!condition.dateTimeUtc1 && !condition.dateTimeUtc2) return condition.type;
    else if (!condition.dateTimeUtc1) return `Before ${formatDate(condition.dateTimeUtc2)}`;
    else if (!condition.dateTimeUtc2) return `After ${formatDate(condition.dateTimeUtc1)}`;
    return `Between ${formatDate(condition.dateTimeUtc1)} and ${formatDate(condition.dateTimeUtc2)}`;
  } else if (condition.type == ConditionType.TimeRange) {
    if (!condition.timeOnlyUtc1 && !condition.timeOnlyUtc2) return condition.type;
    else if (!condition.timeOnlyUtc1) return `Before ${formatTime(condition.timeOnlyUtc2)}`;
    else if (!condition.timeOnlyUtc2) return `After ${formatTime(condition.timeOnlyUtc1)}`;
    return `Between ${formatTime(condition.timeOnlyUtc1)} and ${formatTime(condition.timeOnlyUtc2)}`;
  } else if (condition.type == ConditionType.WeekDays) {
    return !condition.daysOfWeekUtc ? "Weekdays: None" : `${condition.daysOfWeekUtc}`;
  } else if (condition.type == ConditionType.IPEquals) {
    return `When IP matches '${condition.ipCondition}'`;
  } else if (condition.type == ConditionType.IPRegex) {
    return `When IP matches regex '${condition.ipCondition}'`;
  } else if (condition.type == ConditionType.IPCIDRRange) {
    return `When IP is within CIDR range'${condition.ipCondition}'`;
  } else if (condition.type == ConditionType.IsLocalRequest) {
    return `When request is local`;
  } else return condition.type;
}

function formatDate(raw: Date | string): string {
  return DateFormats.defaultDateTime(raw);
}

function formatTime(raw: Date | string): string {
  return raw?.toString();
}
