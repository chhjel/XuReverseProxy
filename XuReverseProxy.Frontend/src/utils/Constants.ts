import { ProxyAuthenticationConditionType } from "@generated/Enums/Core/ProxyAuthenticationConditionType";

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
