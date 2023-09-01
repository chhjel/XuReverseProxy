import ServiceBase, { LoadStatus } from "./ServiceBase";

export default class Base32UtilService extends ServiceBase {
    public async CreateSecretAsync(status: LoadStatus | null = null): Promise<string> {
        const url = `/api/base32Util/createSecret`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<string>(request, status);
        return result.data;
    }
}
