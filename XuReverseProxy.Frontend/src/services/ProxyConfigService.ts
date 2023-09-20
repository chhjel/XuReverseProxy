import EFCrudServiceBase from "./EFCrudServiceBase";
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";

export default class ProxyConfigService extends EFCrudServiceBase<ProxyConfig> {
  constructor() {
    super("proxyConfig");
  }
}
