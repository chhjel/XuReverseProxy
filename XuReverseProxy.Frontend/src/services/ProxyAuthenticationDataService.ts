import { ProxyAuthenticationData } from '@generated/Models/Core/ProxyAuthenticationData';
import EFCrudServiceBase from './EFCrudServiceBase';
import { GenericResultData } from '@generated/Models/Web/GenericResultData';
import { LoadStatus } from './ServiceBase';

export default class ProxyAuthenticationDataService extends EFCrudServiceBase<ProxyAuthenticationData> {
    constructor() {
        super('proxyAuthenticationData');
    }
    
    public async GetFromConfigAsync(configId: string, status: LoadStatus | null = null): Promise<GenericResultData<Array<ProxyAuthenticationData>>> {
        const url = `${this._baseUrl}/fromConfig/${configId}`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<GenericResultData<Array<ProxyAuthenticationData>>>(request, status);
        return result.data;
    }
}
