//     This code was generated by a Reinforced.Typings tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { ProxyClientIdentitySolvedChallengeData } from './ProxyClientIdentitySolvedChallengeData';

export interface ProxyClientIdentity
{
	id: string;
	ip: string;
	userAgent: string;
	note: string;
	createdAtUtc: Date;
	lastAttemptedAccessedAtUtc: Date;
	lastAccessedAtUtc: Date;
	blocked: boolean;
	blockedAtUtc: Date;
	blockedMessage: string;
	solvedChallenges: ProxyClientIdentitySolvedChallengeData[];
	data: any[];
}
