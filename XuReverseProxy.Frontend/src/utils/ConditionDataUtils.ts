import { ConditionType } from "@generated/Enums/Core/ConditionType";
import { ConditionData } from "@generated/Models/Core/ConditionData";
import DateFormats from "./DateFormats";

export function createConditionDataSummary(condition: ConditionData): string {
  const prefix = (upper: string) => (condition.inverted ? `Not ${upper.toLowerCase()}` : upper);

  if (condition.type == ConditionType.DateTimeRange) {
    if (!condition.dateTimeUtc1 && !condition.dateTimeUtc2) return condition.type;
    else if (!condition.dateTimeUtc1) return `${prefix("Before")} ${formatDate(condition.dateTimeUtc2)}`;
    else if (!condition.dateTimeUtc2) return `${prefix("After")} ${formatDate(condition.dateTimeUtc1)}`;
    return `${prefix("Between")} ${formatDate(condition.dateTimeUtc1)} and ${formatDate(condition.dateTimeUtc2)}`;
  } else if (condition.type == ConditionType.TimeRange) {
    if (!condition.timeOnlyUtc1 && !condition.timeOnlyUtc2) return condition.type;
    else if (!condition.timeOnlyUtc1) return `${prefix("Before")} ${formatTime(condition.timeOnlyUtc2)}`;
    else if (!condition.timeOnlyUtc2) return `${prefix("After")} ${formatTime(condition.timeOnlyUtc1)}`;
    return `${prefix("Between")} ${formatTime(condition.timeOnlyUtc1)} and ${formatTime(condition.timeOnlyUtc2)}`;
  } else if (condition.type == ConditionType.WeekDays) {
    return !condition.daysOfWeekUtc
      ? `Weekdays: ${condition.inverted ? "Any" : "None"}`
      : `${prefix("")}${condition.daysOfWeekUtc}`;
  } else if (condition.type == ConditionType.IPEquals) {
    return `${prefix("When")} IP matches '${condition.ipCondition}'`;
  } else if (condition.type == ConditionType.IPRegex) {
    return `${prefix("When")} IP matches regex '${condition.ipCondition}'`;
  } else if (condition.type == ConditionType.IPCIDRRange) {
    return `${prefix("When")} IP is within CIDR range'${condition.ipCondition}'`;
  } else if (condition.type == ConditionType.IsLocalRequest) {
    return `${prefix("When")} request is local`;
  } else return condition.type;
}

function formatDate(raw: Date | string): string {
  return DateFormats.defaultDateTime(raw);
}

function formatTime(raw: Date | string): string {
  return raw?.toString();
}
