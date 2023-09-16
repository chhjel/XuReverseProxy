import { ProxyAuthenticationConditionType } from "@generated/Enums/Core/ProxyAuthenticationConditionType";

export const LoggedOutMessage: string = 'You have been logged out, please refresh the page if you want to continue.';
export const LoggedOutMessage_IpChanged: string = 'Your login session has been terminated due to activity detected from a different IP address. For security purposes, all active sessions have been logged out. Please refresh the page if you want to continue.';

export const EmptyGuid: string = '00000000-0000-0000-0000-000000000000';

export interface ProxyAuthChallengeTypeOption {
    typeId: string;
    name: string;
}
export const ProxyAuthChallengeTypeOptions: Array<ProxyAuthChallengeTypeOption> = [
    {
        typeId: 'ProxyChallengeTypeLogin',
        name: 'Login',
    },
    {
        typeId: 'ProxyChallengeTypeAdminLogin',
        name: 'Admin login',
    },
    {
        typeId: 'ProxyChallengeTypeManualApproval',
        name: 'Manual approval',
    },
    {
        typeId: 'ProxyChallengeTypeOTP',
        name: 'One-time password',
    },
    {
        typeId: 'ProxyChallengeTypeSecretQueryString',
        name: 'Querystring secret',
    }
];

export interface ProxyAuthConditionTypeOption {
    value: ProxyAuthenticationConditionType;
    name: string;
}
export const ProxyAuthConditionTypeOptions: Array<ProxyAuthConditionTypeOption> = [
    {
        value: ProxyAuthenticationConditionType.DateTimeRange,
        name: 'Date range',
    },
    {
        value: ProxyAuthenticationConditionType.TimeRange,
        name: 'Time range',
    },
    {
        value: ProxyAuthenticationConditionType.WeekDays,
        name: 'Week days',
    }
];

export interface PlaceholderGroupInfo {
    name: string;
    placeholders: Array<PlaceholderInfo>;
}
export interface PlaceholderInfo {
    name: string;
    description: string;
}
export const ProxyConfigPlaceholders: Array<PlaceholderInfo> = [
    { name: 'ProxyConfig.Name', description: 'Name of the config.' },
    { name: 'ProxyConfig.Subdomain', description: 'Subdomain where the proxy is configured, or empty if its using the root domain.' },
    { name: 'ProxyConfig.Port', description: 'Port-number the proxy is configured to listen to, or empty for any port.' },
    { name: 'ProxyConfig.ChallengeTitle', description: 'Name of the proxy config displayed on the challenge page.' },
    { name: 'ProxyConfig.ChallengeDescription', description: 'Description of the proxy config displayed on the challenge page.' },
    { name: 'ProxyConfig.DestinationPrefix', description: 'Proxy target.' }
];
export const AuthDataPlaceholders: Array<PlaceholderInfo> = [
    { name: 'Auth.ChallengeTypeId', description: 'Id of the auth challenge type.' }
];
export const ClientIdentityPlaceholders: Array<PlaceholderInfo> = [
    { name: 'Client.IP', description: 'IP of the client.' },
    { name: 'Client.Note', description: 'Client note if any.' },
    { name: 'Client.UserAgent', description: 'UserAgent value of the client.' },
    { name: 'Client.BlockedMessage', description: 'Text entered when blocking the client if any.' }
];
export const CommonPlaceholders: Array<PlaceholderInfo> = [
    { name: 'Weekday', description: 'Current day of the week. E.g. Monday.' },
    { name: 'MonthName', description: 'Name of the current month.' },
    { name: 'DayOfMonth', description: 'Current day of the month.' },
    { name: 'Hour', description: 'Current hour (24h).' },
    { name: 'Minute', description: 'Current minute.' },
    { name: 'Month', description: 'Current month number.' },
    { name: 'Year', description: 'Current year.' }
];

export const ClientBlockedHtmlPlaceholders: Array<PlaceholderGroupInfo> = [
    { name: 'Blocked client', placeholders: ClientIdentityPlaceholders },
    { name: 'Common', placeholders: CommonPlaceholders }
];
export const LoginUsernamePasswordPlaceholders: Array<PlaceholderGroupInfo> = [
    { name: 'Client', placeholders: ClientIdentityPlaceholders },
    { name: 'Proxy config', placeholders: ProxyConfigPlaceholders },
    { name: 'Common', placeholders: CommonPlaceholders }
];
export const ManualApprovalUrlPlaceholders: Array<PlaceholderGroupInfo> = [
    { name: 'Client', placeholders: ClientIdentityPlaceholders },
    { name: 'Proxy config', placeholders: ProxyConfigPlaceholders },
    { name: 'Common', placeholders: CommonPlaceholders }
];
export const OTPRequestUrlPlaceholders: Array<PlaceholderGroupInfo> = [
    { name: 'Client', placeholders: ClientIdentityPlaceholders },
    { name: 'Proxy config', placeholders: ProxyConfigPlaceholders },
    { name: 'Common', placeholders: CommonPlaceholders }
];
