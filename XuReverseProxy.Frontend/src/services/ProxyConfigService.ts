import { GenericResultData } from './../generated/Models/Web/GenericResultData';
import ServiceBase, { LoadStatus } from "./ServiceBase";
import { ProxyConfig } from '@generated/Models/Core/ProxyConfig';

export default class ProxyConfigService extends ServiceBase {

    public async GetProxyConfigsAsync(status: LoadStatus | null = null): Promise<GenericResultData<Array<ProxyConfig>>> {
        const url = `/api/proxyconfig`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<GenericResultData<Array<ProxyConfig>>>(request, status);
        return result.data;
    }

    public async GetProxyConfigAsync(id: string, status: LoadStatus | null = null): Promise<GenericResultData<ProxyConfig>> {
        const url = `/api/proxyconfig/${id}`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<GenericResultData<ProxyConfig>>(request, status);
        return result.data;
    }
}
