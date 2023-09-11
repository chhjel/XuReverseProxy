import { IPLookupResult } from "@generated/Models/Core/IPLookupResult";
import ServiceBase, { LoadStatus } from "./ServiceBase";

export default class IPLookupService extends ServiceBase {
    public async LookupIPAsync(ip: string, status: LoadStatus | null = null): Promise<IPLookupResult> {
        const url = `/api/ipLookup/lookup`;
        const request = this.fetchExt(url, "POST", ip);
        const result = await this.awaitWithStatus<IPLookupResult>(request, status);
        return result.data;
    }
}
