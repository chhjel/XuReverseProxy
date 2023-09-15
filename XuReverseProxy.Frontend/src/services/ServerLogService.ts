import { LoggedEvent } from '../generated/Models/Core/LoggedEvent';
import ServiceBase, { LoadStatus } from "./ServiceBase";

export default class ServerLogService extends ServiceBase {
    public async GetLogAsync(status: LoadStatus | null = null): Promise<Array<LoggedEvent>> {
        const url = `/api/serverLog`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<Array<LoggedEvent>>(request, status);
        return result.data;
    }
}
