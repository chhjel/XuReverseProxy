import { CreateAccountResponse } from './../generated/Models/Web/CreateAccountResponse';
import { CreateAccountRequest } from './../generated/Models/Web/CreateAccountRequest';
import { LoginRequest } from "@generated/Models/Web/LoginRequest";
import { LoginResponse } from "@generated/Models/Web/LoginResponse";
import ServiceBase, { LoadStatus } from "./ServiceBase";

export default class LoginService extends ServiceBase {
    public async LoginAsync(username: string, password: string, returnPath: string, totp: string, status: LoadStatus | null = null): Promise<LoginResponse> {
        const payload: LoginRequest = {
            username: username,
            password: password,
            returnPath: returnPath,
            totp: totp,
            recoveryCode: ''
        };
        const url = `/auth/login`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<LoginResponse>(request, status);
        return result.data;
    }

    public async CreateAccountAsync(username: string, password: string, totpSecret: string | null, totpCode: string | null, status: LoadStatus | null = null): Promise<CreateAccountResponse> {
        const payload: CreateAccountRequest = {
            username: username,
            password: password,
            totpSecret: totpSecret,
            totpCode: totpCode
        };
        const url = `/auth/create`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<CreateAccountResponse>(request, status);
        return result.data;
    }
}
