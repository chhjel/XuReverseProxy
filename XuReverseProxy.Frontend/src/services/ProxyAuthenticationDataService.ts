import { ProxyAuthenticationData } from "@generated/Models/Core/ProxyAuthenticationData";
import { ProxyAuthenticationDataOrderData } from "@generated/Models/Web/ProxyAuthenticationDataOrderData";
import EFCrudServiceBase from "./EFCrudServiceBase";
import { GenericResultData } from "@generated/Models/Web/GenericResultData";
import { LoadStatus } from "./ServiceBase";
import { GenericResult } from "@generated/Models/Web/GenericResult";

export default class ProxyAuthenticationDataService extends EFCrudServiceBase<ProxyAuthenticationData> {
  constructor() {
    super("proxyAuthenticationData");
  }

  public async GetFromConfigAsync(
    configId: string,
    status: LoadStatus | null = null,
  ): Promise<GenericResultData<Array<ProxyAuthenticationData>>> {
    const url = `${this._baseUrl}/fromConfig/${configId}`;
    const request = this.fetchExt(url, "GET", null);
    const result = await this.awaitWithStatus<GenericResultData<Array<ProxyAuthenticationData>>>(request, status);
    return result.data;
  }

  public async UpdateAuthOrdersAsync(
    items: Array<ProxyAuthenticationDataOrderData>,
    status: LoadStatus | null = null,
  ): Promise<GenericResult> {
    const url = `${this._baseUrl}/updateOrder`;
    const request = this.fetchExt(url, "POST", items);
    const result = await this.awaitWithStatus<GenericResult>(request, status);
    return result.data;
  }
}
