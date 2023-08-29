import { SetClientBlockedFromManualApprovalRequestMessage } from "@generated/Models/Web/SetClientBlockedFromManualApprovalRequestMessage";
import ServiceBase, { LoadStatus } from "./ServiceBase";
import { SetClientNoteFromManualApprovalRequestMessage } from "@generated/Models/Web/SetClientNoteFromManualApprovalRequestMessage";

export default class ManualApprovalService extends ServiceBase {
    private readonly _clientIdentityId: string;
    private readonly _authenticationId: string;
    private readonly _solvedId: string;
    private readonly _baseUrl: string;

    constructor(clientIdentityId: string, authenticationId: string, solvedId: string) {
        super();
        this._clientIdentityId = clientIdentityId;
        this._authenticationId = authenticationId;
        this._solvedId = solvedId;
        this._baseUrl = `/proxyAuth/approve/${this._clientIdentityId}/${this._authenticationId}/${this._solvedId}`;
    }

    public async ApproveAsync(status: LoadStatus | null = null): Promise<boolean> {
        const url = `${this._baseUrl}/approve`;
        const request = this.fetchExt(url, "POST", null);
        const result = await this.awaitWithStatus<boolean>(request, status);
        return result.data;
    }

    public async UnapproveAsync(status: LoadStatus | null = null): Promise<boolean> {
        const url = `${this._baseUrl}/unapprove`;
        const request = this.fetchExt(url, "POST", null);
        const result = await this.awaitWithStatus<boolean>(request, status);
        return result.data;
    }

    public async SetClientBlockedAsync(payload: SetClientBlockedFromManualApprovalRequestMessage, status: LoadStatus | null = null): Promise<boolean> {
        const url = `${this._baseUrl}/block`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<boolean>(request, status);
        return result.data;
    }

    public async SetClientNoteAsync(payload: SetClientNoteFromManualApprovalRequestMessage, status: LoadStatus | null = null): Promise<boolean> {
        const url = `${this._baseUrl}/setclientnote`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<boolean>(request, status);
        return result.data;
    }
}
