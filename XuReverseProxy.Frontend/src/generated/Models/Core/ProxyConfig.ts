//     This code was generated by a Reinforced.Typings tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { ProxyConfigMode } from '../../Enums/Core/ProxyConfigMode';
import { ProxyAuthenticationData } from './ProxyAuthenticationData';
import { ConditionData } from './ConditionData';
import { HtmlTemplate } from './HtmlTemplate';

export interface ProxyConfig
{
	id: string;
	enabled: boolean;
	name: string;
	subdomain: string;
	port: number;
	challengeTitle: string;
	challengeDescription: string;
	showCompletedChallenges: boolean;
	showChallengesWithUnmetRequirements: boolean;
	mode: ProxyConfigMode;
	destinationPrefix: string;
	staticHTML: string;
	rewriteDownstreamOrigin: boolean;
	stripUpstreamSourceTraces: boolean;
	showConditionsNotMet: boolean;
	authentications: ProxyAuthenticationData[];
	proxyConditions: ConditionData[];
	htmlTemplateOverrides: HtmlTemplate[];
}
