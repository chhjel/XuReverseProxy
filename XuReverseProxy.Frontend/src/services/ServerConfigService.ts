import { LoadStatus } from "./ServiceBase";
import { RuntimeServerConfigItem } from "./../generated/Models/Core/RuntimeServerConfigItem";
import EFCrudServiceBase from "./EFCrudServiceBase";

export default class ServerConfigService extends EFCrudServiceBase<RuntimeServerConfigItem> {
  constructor() {
    super("serverConfig");
  }

  public async IsConfigFlagEnabledAsync(key: string, status: LoadStatus | null = null): Promise<boolean> {
    const value = await this.GetConfigValueAsync(key, status);
    return value?.toLowerCase() == "true";
  }

  public async GetConfigValueAsync(key: string, status: LoadStatus | null = null): Promise<string> {
    const url = `${this._baseUrl}/configValue`;
    const request = this.fetchExt(url, "POST", key);
    const result = await this.awaitWithStatus<string>(request, status, true, "configValue");
    return result.data;
  }
}
