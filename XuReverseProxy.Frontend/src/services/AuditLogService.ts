import { PaginatedResult } from '@generated/Models/Web/PaginatedResult';
import { GetAdminAuditLogEntriesRequestModel } from '@generated/Models/Web/GetAdminAuditLogEntriesRequestModel';
import { GetClientAuditLogEntriesRequestModel } from '@generated/Models/Web/GetClientAuditLogEntriesRequestModel';
import ServiceBase, { LoadStatus } from "./ServiceBase";
import { AdminAuditLogEntry } from '@generated/Models/Core/AdminAuditLogEntry';
import { ClientAuditLogEntry } from '@generated/Models/Core/ClientAuditLogEntry';

export default class AuditLogService extends ServiceBase {
    public async GetAdminLogAsync(payload: GetAdminAuditLogEntriesRequestModel, status: LoadStatus | null = null): Promise<PaginatedResult<AdminAuditLogEntry>> {
        const url = `/api/auditLog/adminLog`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<PaginatedResult<AdminAuditLogEntry>>(request, status);
        return result.data;
    }

    public async GetClientLogAsync(payload: GetClientAuditLogEntriesRequestModel, status: LoadStatus | null = null): Promise<PaginatedResult<ClientAuditLogEntry>> {
        const url = `/api/auditLog/clientLog`;
        const request = this.fetchExt(url, "POST", payload);
        const result = await this.awaitWithStatus<PaginatedResult<ClientAuditLogEntry>>(request, status);
        return result.data;
    }
}
