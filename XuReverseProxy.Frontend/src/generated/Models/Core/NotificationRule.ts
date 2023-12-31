//     This code was generated by a Reinforced.Typings tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

import { NotificationTrigger } from '../../Enums/Core/NotificationTrigger';
import { NotificationAlertType } from '../../Enums/Core/NotificationAlertType';

export interface NotificationRule
{
	id: string;
	name: string;
	enabled: boolean;
	triggerType: NotificationTrigger;
	alertType: NotificationAlertType;
	webHookUrl: string;
	webHookMethod: string;
	webHookHeaders: string;
	webHookBody: string;
	cooldownDistinctPattern: string;
	cooldown: string;
	lastNotifiedAtUtc: Date;
	lastNotifyResult: string;
}
