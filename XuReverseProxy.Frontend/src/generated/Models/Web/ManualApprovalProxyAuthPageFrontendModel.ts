//     This code was generated by a Reinforced.Typings tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { ClientDataFrontendModel } from './ClientDataFrontendModel';
import { CurrentChallengeDataFrontendModel } from './CurrentChallengeDataFrontendModel';
import { ChallengeDataFrontendModel } from './ChallengeDataFrontendModel';
import { ProxyConfigFrontendModel } from './ProxyConfigFrontendModel';

export interface ManualApprovalProxyAuthPageFrontendModel
{
	authenticationId: string;
	solvedId: string;
	isLoggedIn: boolean;
	client: ClientDataFrontendModel;
	currentChallengeData: CurrentChallengeDataFrontendModel;
	allChallengeData: ChallengeDataFrontendModel[];
	proxyConfig: ProxyConfigFrontendModel;
}
