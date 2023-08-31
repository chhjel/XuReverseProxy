
export interface ProxyAuthChallengeType {
    typeId: string;
    name: string;
}

export const ProxyAuthChallengeTypes: Array<ProxyAuthChallengeType> = [
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
