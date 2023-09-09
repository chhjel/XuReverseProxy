import { SetClientNoteRequestModel } from './../generated/Models/Web/SetClientNoteRequestModel';
import { SetClientBlockedRequestModel } from './../generated/Models/Web/SetClientBlockedRequestModel';
import { ProxyClientIdentity } from './../generated/Models/Core/ProxyClientIdentity';
import EFCrudServiceBase from './EFCrudServiceBase';
import { LoadStatus } from './ServiceBase';
import { GenericResult } from '@generated/Models/Web/GenericResult';


export default class ProxyClientIdentityService extends EFCrudServiceBase<ProxyClientIdentity> {
    constructor() {
        super('proxyClientIdentity');
    }

    public async SetClientNoteAsync(payload: SetClientNoteRequestModel, status: LoadStatus | null = null): Promise<GenericResult> {
        const url = `${this._baseUrl}/setNote`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<GenericResult>(request, status);
        return result.data;
    }

    public async SetClientBlockedAsync(payload: SetClientBlockedRequestModel, status: LoadStatus | null = null): Promise<GenericResult> {
        const url = `${this._baseUrl}/setBlocked`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<GenericResult>(request, status);
        return result.data;
    }
}
