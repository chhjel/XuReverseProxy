import { DayOfWeekFlags } from "@generated/Enums/Core/DayOfWeekFlags";
import { BlockedIpDataType } from "./../generated/Enums/Core/BlockedIpDataType";
import { NotificationAlertType } from "./../generated/Enums/Core/NotificationAlertType";
import { NotificationTrigger } from "./../generated/Enums/Core/NotificationTrigger";
import { ProxyAuthenticationConditionType } from "@generated/Enums/Core/ProxyAuthenticationConditionType";

// eslint-disable-next-line no-var
declare var PRODUCTION: boolean;
// eslint-disable-next-line no-var
declare var DEVELOPMENT: boolean;

export const isProductionEnv: boolean = PRODUCTION;
export const isDevelopmentEnv: boolean = DEVELOPMENT;

export const LoggedOutMessage: string = "You have been logged out, please refresh the page if you want to continue.";
export const LoggedOutMessageIpChanged: string =
  "Your login session has been terminated due to activity detected from a different IP address. For security purposes, all active sessions have been logged out. Please refresh the page if you want to continue.";

export const EmptyGuid: string = "00000000-0000-0000-0000-000000000000";

export const HttpRequestMethodOptions: Array<any> = [
  { value: "GET", name: "GET" },
  { value: "POST", name: "POST" },
  { value: "PUT", name: "PUT" },
  { value: "DELETE", name: "DELETE" },
  { value: "HEAD", name: "HEAD" },
  { value: "CONNECT", name: "CONNECT" },
  { value: "OPTIONS", name: "OPTIONS" },
  { value: "TRACE", name: "TRACE" },
];

export interface ProxyAuthChallengeTypeOption {
  typeId: string;
  name: string;
}
export const ProxyAuthChallengeTypeOptions: Array<ProxyAuthChallengeTypeOption> = [
  {
    typeId: "ProxyChallengeTypeLogin",
    name: "Login",
  },
  {
    typeId: "ProxyChallengeTypeAdminLogin",
    name: "Admin login",
  },
  {
    typeId: "ProxyChallengeTypeManualApproval",
    name: "Manual approval",
  },
  {
    typeId: "ProxyChallengeTypeOTP",
    name: "One-time password",
  },
  {
    typeId: "ProxyChallengeTypeSecretQueryString",
    name: "Querystring secret",
  },
];

export interface ProxyAuthConditionTypeOption {
  value: ProxyAuthenticationConditionType;
  name: string;
}
export const ProxyAuthConditionTypeOptions: Array<ProxyAuthConditionTypeOption> = [
  {
    value: ProxyAuthenticationConditionType.DateTimeRange,
    name: "Date range",
  },
  {
    value: ProxyAuthenticationConditionType.TimeRange,
    name: "Time range",
  },
  {
    value: ProxyAuthenticationConditionType.WeekDays,
    name: "Week days",
  },
];

export interface NotificationAlertTypeOption {
  value: NotificationAlertType;
  name: string;
}
export const NotificationAlertTypeOptions: Array<NotificationAlertTypeOption> = [
  { value: NotificationAlertType.WebHook, name: "WebHook" },
];
export interface NotificationTriggerOption {
  value: NotificationTrigger;
  name: string;
}
export const NotificationTriggerOptions: Array<NotificationTriggerOption> = [
  { value: NotificationTrigger.AdminLoginSuccess, name: "Admin login" },
  { value: NotificationTrigger.AdminLoginFailed, name: "Admin login failed" },
  { value: NotificationTrigger.AdminRequests, name: "Admin request" },
  {
    value: NotificationTrigger.AdminSessionIPChanged,
    name: "Admin session IP changed",
  },
  { value: NotificationTrigger.NewClient, name: "New client created" },
  { value: NotificationTrigger.ClientRequest, name: "Client request" },
  {
    value: NotificationTrigger.ClientCompletedChallenge,
    name: "Client completed an authentication challenge",
  },
];

export interface BlockedIpDataTypeOption {
  value: BlockedIpDataType;
  name: string;
}
export const BlockedIpDataTypeOptions: Array<BlockedIpDataTypeOption> = [
  { value: BlockedIpDataType.IP, name: "Single IP" },
  { value: BlockedIpDataType.IPRegex, name: "RegEx" },
  { value: BlockedIpDataType.CIDRRange, name: "CIDR range" },
];

//////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////
export interface PlaceholderGroupInfo {
  name: string;
  placeholders: Array<PlaceholderInfo>;
}
export interface PlaceholderInfo {
  name: string;
  description: string;
}

export const ProxyConfigPlaceholders: Array<PlaceholderInfo> = [
  { name: "ProxyConfig.Id", description: "Id of the config." },
  { name: "ProxyConfig.Name", description: "Name of the config." },
  {
    name: "ProxyConfig.Subdomain",
    description: "Subdomain where the proxy is configured, or empty if its using the root domain.",
  },
  {
    name: "ProxyConfig.Port",
    description: "Port-number the proxy is configured to listen to, or empty for any port.",
  },
  {
    name: "ProxyConfig.ChallengeTitle",
    description: "Name of the proxy config displayed on the challenge page.",
  },
  {
    name: "ProxyConfig.ChallengeDescription",
    description: "Description of the proxy config displayed on the challenge page.",
  },
  { name: "ProxyConfig.DestinationPrefix", description: "Proxy target." },
];
export const AuthDataPlaceholders: Array<PlaceholderInfo> = [
  { name: "Auth.Id", description: "Id of the auth." },
  {
    name: "Auth.ChallengeTypeId",
    description: "Id of the auth challenge type.",
  },
];
export const ClientIdentityPlaceholders: Array<PlaceholderInfo> = [
  { name: "Client.Id", description: "Id of the client." },
  { name: "Client.Note", description: "Client note if any." },
  { name: "Client.IP", description: "IP of the client." },
  { name: "Client.UserAgent", description: "UserAgent value of the client." },
  {
    name: "Client.BlockedMessage",
    description: "Text entered when blocking the client if any.",
  },
];
export const UserPlaceholders: Array<PlaceholderInfo> = [
  { name: "User.Id", description: "Id of the user." },
  { name: "User.Username", description: "Username." },
  { name: "User.IP", description: "Latest IP of the user." },
];
export const CommonPlaceholders: Array<PlaceholderInfo> = [
  { name: "Weekday", description: "Current day of the week. E.g. Monday." },
  { name: "MonthName", description: "Name of the current month." },
  { name: "DayOfMonth", description: "Current day of the month." },
  { name: "Hour", description: "Current hour (24h)." },
  { name: "Minute", description: "Current minute." },
  { name: "Month", description: "Current month number." },
  { name: "Year", description: "Current year." },
  { name: "GUID", description: "A random GUID." },
];

export const WeekdayOptions: Array<DayOfWeekFlags> = [
  DayOfWeekFlags.Monday,
  DayOfWeekFlags.Tuesday,
  DayOfWeekFlags.Wednesday,
  DayOfWeekFlags.Thursday,
  DayOfWeekFlags.Friday,
  DayOfWeekFlags.Saturday,
  DayOfWeekFlags.Sunday,
];

//////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////
export const IPBlockedHtmlPlaceholders: Array<PlaceholderGroupInfo> = [
  { name: "Common", placeholders: CommonPlaceholders },
];
export const Html404Placeholders: Array<PlaceholderGroupInfo> = [{ name: "Common", placeholders: CommonPlaceholders }];
export const ClientBlockedHtmlPlaceholders: Array<PlaceholderGroupInfo> = [
  { name: "Blocked client", placeholders: ClientIdentityPlaceholders },
  { name: "Common", placeholders: CommonPlaceholders },
];
export const LoginUsernamePasswordPlaceholders: Array<PlaceholderGroupInfo> = [
  { name: "Client", placeholders: ClientIdentityPlaceholders },
  { name: "Proxy config", placeholders: ProxyConfigPlaceholders },
  { name: "Common", placeholders: CommonPlaceholders },
];
export const ManualApprovalUrlPlaceholders: Array<PlaceholderGroupInfo> = [
  { name: "Client", placeholders: ClientIdentityPlaceholders },
  { name: "Proxy config", placeholders: ProxyConfigPlaceholders },
  { name: "Common", placeholders: CommonPlaceholders },
];
export const OTPRequestUrlPlaceholders: Array<PlaceholderGroupInfo> = [
  { name: "Client", placeholders: ClientIdentityPlaceholders },
  { name: "Proxy config", placeholders: ProxyConfigPlaceholders },
  { name: "Common", placeholders: CommonPlaceholders },
];

//////////////////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////

export const AdminLoginSuccessNotificationPlaceholders: Array<PlaceholderGroupInfo> = [
  {
    name: "",
    placeholders: [{ name: "Username", description: "Username that logged in." }],
  },
  { name: "Common", placeholders: CommonPlaceholders },
];
export const AdminLoginFailedNotificationPlaceholders: Array<PlaceholderGroupInfo> = [
  {
    name: "",
    placeholders: [{ name: "Username", description: "Username that failed to login." }],
  },
  { name: "Common", placeholders: CommonPlaceholders },
];
export const AdminRequestsNotificationPlaceholders: Array<PlaceholderGroupInfo> = [
  { name: "", placeholders: [{ name: "Url", description: "Current url." }] },
  { name: "User", placeholders: UserPlaceholders },
  { name: "Common", placeholders: CommonPlaceholders },
];
export const AdminSessionIPChangedNotificationPlaceholders: Array<PlaceholderGroupInfo> = [
  {
    name: "",
    placeholders: [
      { name: "OldIP", description: "The old IP." },
      { name: "NewIP", description: "The new IP." },
    ],
  },
  { name: "User", placeholders: UserPlaceholders },
  { name: "Common", placeholders: CommonPlaceholders },
];
export const NewClientNotificationPlaceholders: Array<PlaceholderGroupInfo> = [
  { name: "", placeholders: [{ name: "Url", description: "Current url." }] },
  { name: "Client", placeholders: ClientIdentityPlaceholders },
  { name: "Common", placeholders: CommonPlaceholders },
];
export const ClientRequestNotificationPlaceholders: Array<PlaceholderGroupInfo> = [
  { name: "", placeholders: [{ name: "Url", description: "Current url." }] },
  { name: "Client", placeholders: ClientIdentityPlaceholders },
  { name: "Proxy config", placeholders: ProxyConfigPlaceholders },
  { name: "Common", placeholders: CommonPlaceholders },
];
export const ClientCompletedChallengeNotificationPlaceholders: Array<PlaceholderGroupInfo> = [
  { name: "Client", placeholders: ClientIdentityPlaceholders },
  { name: "Proxy config", placeholders: ProxyConfigPlaceholders },
  { name: "Auth data", placeholders: AuthDataPlaceholders },
  { name: "Common", placeholders: CommonPlaceholders },
];

export function getPlaceholdersForNotificationType(type: NotificationTrigger): Array<PlaceholderGroupInfo> {
  if (type == NotificationTrigger.AdminLoginSuccess) return AdminLoginSuccessNotificationPlaceholders;
  else if (type == NotificationTrigger.AdminLoginFailed) return AdminLoginFailedNotificationPlaceholders;
  else if (type == NotificationTrigger.AdminRequests) return AdminRequestsNotificationPlaceholders;
  else if (type == NotificationTrigger.AdminSessionIPChanged) return AdminSessionIPChangedNotificationPlaceholders;
  else if (type == NotificationTrigger.NewClient) return NewClientNotificationPlaceholders;
  else if (type == NotificationTrigger.ClientRequest) return ClientRequestNotificationPlaceholders;
  else if (type == NotificationTrigger.ClientCompletedChallenge)
    return ClientCompletedChallengeNotificationPlaceholders;
  else return [];
}
