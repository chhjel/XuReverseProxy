//     This code was generated by a Reinforced.Typings tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.

export interface ClientAuditLogEntry
{
	id: string;
	timestampUtc: Date;
	clientId: string;
	ip: string;
	action: string;
	relatedProxyConfigId: string;
	relatedProxyConfigName: string;
}