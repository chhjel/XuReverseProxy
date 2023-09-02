import ServiceBase, { LoadStatus } from "./ServiceBase";
import { GenericResultData } from "@generated/Models/Web/GenericResultData";
import { GenericResult } from "@generated/Models/Web/GenericResult";

export default class EFCrudServiceBase<TEntity> extends ServiceBase {
    protected _typeName: string;
    protected _baseUrl: string;

    constructor(typeName: string) {
        super();
        this._typeName = typeName;
        this._baseUrl = `/api/${this._typeName}`;
    }

    public async GetAllAsync(status: LoadStatus | null = null): Promise<GenericResultData<Array<TEntity>>> {
        const url = `${this._baseUrl}/`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<GenericResultData<Array<TEntity>>>(request, status);
        return result.data;
    }

    public async GetAllFullAsync(status: LoadStatus | null = null): Promise<GenericResultData<Array<TEntity>>> {
        const url = `${this._baseUrl}/full`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<GenericResultData<Array<TEntity>>>(request, status);
        return result.data;
    }

    public async GetAsync(entityId: string, status: LoadStatus | null = null): Promise<GenericResultData<TEntity>> {
        const url = `${this._baseUrl}/${entityId}`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<GenericResultData<TEntity>>(request, status);
        return result.data;
    }

    public async GetFullAsync(entityId: string, status: LoadStatus | null = null): Promise<GenericResultData<TEntity>> {
        const url = `${this._baseUrl}/${entityId}/full`;
        const request = this.fetchExt(url, "GET", null);
        const result = await this.awaitWithStatus<GenericResultData<TEntity>>(request, status);
        return result.data;
    }

    public async CreateOrUpdateAsync(entity: TEntity, status: LoadStatus | null = null): Promise<GenericResultData<TEntity>> {
        const url = `${this._baseUrl}/`;
        const request = this.fetchExt(url, "POST", entity);
        const result = await this.awaitWithStatus<GenericResultData<TEntity>>(request, status);
        return result.data;
    }

    public async DeleteAsync(entityId: string, status: LoadStatus | null = null): Promise<GenericResult> {
        const url = `${this._baseUrl}/${entityId}`;
        const request = this.fetchExt(url, "DELETE", null);
        const result = await this.awaitWithStatus<GenericResult>(request, status);
        return result.data;
    }
}
