import { ProxyAuthenticationData } from "@generated/Models/Core/ProxyAuthenticationData";
import { ProxyAuthChallengeTypeOptions } from "./Constants";
import { ProxyChallengeTypeLogin } from "@generated/Models/Core/ProxyChallengeTypeLogin";
import { ProxyChallengeTypeSecretQueryString } from "@generated/Models/Core/ProxyChallengeTypeSecretQueryString";

export function createProxyAuthenticationSummary(auth: ProxyAuthenticationData): string {
  if (auth.challengeTypeId == "ProxyChallengeTypeSecretQueryString") {
    const data = JSON.parse(auth.challengeJson) as ProxyChallengeTypeSecretQueryString;
    return `Require query string secret '${data.secret}'`;
  } else if (auth.challengeTypeId == "ProxyChallengeTypeLogin") {
    const data = JSON.parse(auth.challengeJson) as ProxyChallengeTypeLogin;
    const parts: Array<string> = [];
    if (data.username != null && data.username.trim().length > 0) parts.push("username");
    if (data.password != null && data.password.trim().length > 0) parts.push("password");
    if (data.totpSecret != null && data.totpSecret.trim().length > 0) parts.push("TOTP");
    return `Require login (${parts.join(", ")})`;
  } else if (auth.challengeTypeId == "ProxyChallengeTypeAdminLogin") {
    return `Require admin login`;
  } else if (auth.challengeTypeId == "ProxyChallengeTypeOTP") return `Require one-time password`;
  else if (auth.challengeTypeId == "ProxyChallengeTypeManualApproval") return `Require manual approval`;
  else return auth.challengeTypeId;
}

export function getProxyAuthenticationTypeName(typeId: string): string {
  return ProxyAuthChallengeTypeOptions.find((x) => x.typeId == typeId)?.name || typeId;
}
