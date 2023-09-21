//     This code was generated by a Reinforced.Typings tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { ProxyAuthenticationData } from './ProxyAuthenticationData';
import { ProxyAuthenticationConditionType } from '../../Enums/Core/ProxyAuthenticationConditionType';
import { DayOfWeekFlags } from '../../Enums/Core/DayOfWeekFlags';

export interface ProxyAuthenticationCondition
{
	id: string;
	authenticationDataId: string;
	authenticationData: ProxyAuthenticationData;
	conditionType: ProxyAuthenticationConditionType;
	dateTimeUtc1: Date;
	dateTimeUtc2: Date;
	timeOnlyUtc1: string;
	timeOnlyUtc2: string;
	daysOfWeekUtc: DayOfWeekFlags;
}
