import { ScheduledTaskResult } from './../generated/Models/Core/ScheduledTaskResult';
import { ScheduledTaskStatus } from './../generated/Models/Core/ScheduledTaskStatus';
import ServiceBase, { LoadStatus } from "./ServiceBase";

export default class ScheduledTasksService extends ServiceBase {
    public async GetTaskStatusesAsync(status: LoadStatus | null = null): Promise<Array<ScheduledTaskStatus>> {
        const url = `/api/scheduledTasks/statuses`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<Array<ScheduledTaskStatus>>(request, status);
        return result.data;
    }

    public async GetTaskResultsAsync(status: LoadStatus | null = null): Promise<Array<ScheduledTaskResult>> {
        const url = `/api/scheduledTasks/results`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<Array<ScheduledTaskResult>>(request, status);
        return result.data;
    }
}
