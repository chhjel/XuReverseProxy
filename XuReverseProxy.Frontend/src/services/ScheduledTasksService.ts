import { ScheduledTaskViewModel } from "@generated/Models/Web/ScheduledTaskViewModel";
import ServiceBase, { LoadStatus } from "./ServiceBase";

export default class ScheduledTasksService extends ServiceBase {
  public async GetTasksDetailsAsync(status: LoadStatus | null = null): Promise<Array<ScheduledTaskViewModel>> {
    const url = `/api/scheduledTasks/`;
    const request = this.fetchExt(url, "GET", null);
    const result = await this.awaitWithStatus<Array<ScheduledTaskViewModel>>(request, status);
    return result.data;
  }
}
