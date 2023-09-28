import { TestCidrRangeRequestModel } from "@generated/Models/Web/TestCidrRangeRequestModel";
import { TestRegexRequestModel } from "../generated/Models/Web/TestRegexRequestModel";
import ServiceBase, { LoadStatus } from "./ServiceBase";

export default class MiscUtilsService extends ServiceBase {
  _baseUrl: string = '/api/miscUtils';

  public async IsIPInCidrRangeAsync(
    payload: TestCidrRangeRequestModel,
    status: LoadStatus | null = null,
  ): Promise<boolean> {
    const url = `${this._baseUrl}/isIPInCidrRange`;
    const request = this.fetchExt(url, "POST", payload);
    const result = await this.awaitWithStatus<boolean>(request, status);
    return result.data;
  }

  public async TestRegexAsync(
    payload: TestRegexRequestModel,
    status: LoadStatus | null = null,
  ): Promise<boolean> {
    const url = `${this._baseUrl}/testRegex`;
    const request = this.fetchExt(url, "POST", payload);
    const result = await this.awaitWithStatus<boolean>(request, status);
    return result.data;
  }
}
