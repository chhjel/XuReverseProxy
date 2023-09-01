import { ProxyAuthenticationData } from "@generated/Models/Core/ProxyAuthenticationData";

export function createProxyAuthenticationSummary(auth: ProxyAuthenticationData): string {
    const data = JSON.parse(auth.challengeJson);
    if (auth.challengeTypeId == 'ProxyChallengeTypeSecretQueryString') {
        return `Require query string secret '${data.secret}'`;
    }
    else if (auth.challengeTypeId == 'ProxyChallengeTypeLogin') {
        let parts: Array<string> = [];
        if (data.username != null && data.username.trim().length > 0) parts.push('username');
        if (data.password != null && data.password.trim().length > 0) parts.push('password');
        if (data.totpSecret != null && data.totpSecret.trim().length > 0) parts.push('TOTP');
        return `Require login (${parts.join(', ')})`;
    }
    else if (auth.challengeTypeId == 'ProxyChallengeTypeAdminLogin') {
        return `Require admin login`;
    }
    else if (auth.challengeTypeId == 'ProxyChallengeTypeOTP') return `Require one-time password`;
    else if (auth.challengeTypeId == 'ProxyChallengeTypeManualApproval') return `Require manual approval`;
    else return auth.challengeTypeId;
}
