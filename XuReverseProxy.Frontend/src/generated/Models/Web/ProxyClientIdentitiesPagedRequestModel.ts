//     This code was generated by a Reinforced.Typings tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { ProxyClientsSortBy } from '../../Enums/Web/ProxyClientsSortBy';

export interface ProxyClientIdentitiesPagedRequestModel
{
	pageIndex: number;
	pageSize: number;
	sortBy: ProxyClientsSortBy;
	sortDescending: boolean;
	filter: string;
	ip: string;
	notId: string;
	hasAccessToProxyConfigId: string;
}
