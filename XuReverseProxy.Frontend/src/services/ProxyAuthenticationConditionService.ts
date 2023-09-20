import { ProxyAuthenticationCondition } from "@generated/Models/Core/ProxyAuthenticationCondition";
import EFCrudServiceBase from "./EFCrudServiceBase";
import { LoadStatus } from "./ServiceBase";
import { GenericResultData } from "@generated/Models/Web/GenericResultData";

export default class ProxyAuthenticationConditionService extends EFCrudServiceBase<ProxyAuthenticationCondition> {
  constructor() {
    super("proxyAuthenticationCondition");
  }

  public async GetFromAuthAsync(
    authDataId: string,
    status: LoadStatus | null = null,
  ): Promise<GenericResultData<Array<ProxyAuthenticationCondition>>> {
    const url = `${this._baseUrl}/fromAuthData/${authDataId}`;
    const request = this.fetchExt(url, "GET", null);
    const result = await this.awaitWithStatus<GenericResultData<Array<ProxyAuthenticationCondition>>>(request, status);
    return result.data;
  }
}
