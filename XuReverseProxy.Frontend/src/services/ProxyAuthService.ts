import { LoginResponse } from "@generated/Models/Web/LoginResponse";
import ServiceBase, { LoadStatus } from "./ServiceBase";

export default class ProxyAuthService extends ServiceBase {
    private _challengeTypeId: string;

    constructor(challengeTypeId: string) {
        super();
        this._challengeTypeId = challengeTypeId;
    }

    public async RequestAsync(methodId: string, payload: any, status: LoadStatus | null = null): Promise<any> {
        const url = `/proxyAuth/api/${this._challengeTypeId}/${methodId}`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<LoginResponse>(request, status);
        return result.data;
    }
}
