import { IPMatchesRegexRequestModel } from './../generated/Models/Web/IPMatchesRegexRequestModel';
import { TestCidrRangeRequestModel } from './../generated/Models/Web/TestCidrRangeRequestModel';
import { RemoveIPBlockByIdRequestModel } from './../generated/Models/Web/RemoveIPBlockByIdRequestModel';
import { BlockIPRequestModel } from './../generated/Models/Web/BlockIPRequestModel';
import { BlockIPRegexRequestModel } from './../generated/Models/Web/BlockIPRegexRequestModel';
import { BlockIPCidrRangeRequestModel } from './../generated/Models/Web/BlockIPCidrRangeRequestModel';
import { FetchResult, LoadStatus } from "./ServiceBase";
import { BlockedIpData } from "@generated/Models/Core/BlockedIpData";
import EFCrudServiceBase from './EFCrudServiceBase';

export default class IPBlockService  extends EFCrudServiceBase<BlockedIpData> {
    constructor() {
        super('ipBlock');
    }

    public async IsIPBlockedAsync(ip: string, status: LoadStatus | null = null): Promise<boolean> {
        const url = `${this._baseUrl}/IsIPBlocked`;
        const request = this.fetchExt(url, "POST", ip);
        const result = await this.awaitWithStatus<boolean>(request, status);
        return result.data;
    }

    public async GetMatchingBlockedIpDataForAsync(ip: string, status: LoadStatus | null = null): Promise<BlockedIpData | null> {
        const url = `${this._baseUrl}/GetMatchingBlockedIpDataFor`;
        const request = this.fetchExt(url, "POST", ip);
        const result = await this.awaitWithStatus<BlockedIpData | null>(request, status);
        return result.data;
    }

    public async BlockIPAsync(payload: BlockIPRequestModel, status: LoadStatus | null = null): Promise<BlockedIpData> {
        const url = `${this._baseUrl}/BlockIP`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<BlockedIpData>(request, status);
        return result.data;
    }

    public async BlockIPRegexAsync(payload: BlockIPRegexRequestModel, status: LoadStatus | null = null): Promise<BlockedIpData> {
        const url = `${this._baseUrl}/BlockIPRegex`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<BlockedIpData>(request, status);
        return result.data;
    }

    public async BlockIPCidrRangeAsync(payload: BlockIPCidrRangeRequestModel, status: LoadStatus | null = null): Promise<BlockedIpData> {
        const url = `${this._baseUrl}/BlockIPCidrRange`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<BlockedIpData>(request, status);
        return result.data;
    }

    public async RemoveIPBlockByIdAsync(payload: RemoveIPBlockByIdRequestModel, status: LoadStatus | null = null): Promise<FetchResult<any>> {
        const url = `${this._baseUrl}/RemoveIPBlockById`;
        const request = this.fetchExt(url, "DELETE", payload);
        return await this.awaitWithStatusNoResult(request, status);
    }

    public async IsIPInCidrRangeAsync(payload: TestCidrRangeRequestModel, status: LoadStatus | null = null): Promise<boolean> {
        const url = `${this._baseUrl}/IsIPInCidrRange`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<boolean>(request, status);
        return result.data;
    }

    public async IPMatchesRegexAsync(payload: IPMatchesRegexRequestModel, status: LoadStatus | null = null): Promise<boolean> {
        const url = `${this._baseUrl}/IPMatchesRegex`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<boolean>(request, status);
        return result.data;
    }
}
