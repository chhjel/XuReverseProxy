export default class ServiceBase {
    public static simulatedDelay: number = 0;
    public status: LoadStatus = new LoadStatus();

    protected async awaitWithStatusNoResult(
        promise: Promise<Response>,
        status: LoadStatus | null = null
    ): Promise<FetchResult<any>> {
        return this.awaitWithStatus<any>(promise, status, false);
    }

    protected async awaitWithStatus<T>(
        promise: Promise<Response>,
        status: LoadStatus | null = null,
        json: boolean = true
    ): Promise<FetchResult<T>> {
        return new Promise<FetchResult<T>>(async (resolve, reject) => {
            const statuses = new MultiLoadStatus([ this.status, status ]);
            statuses.setInProgress();
            try {
                const response = await promise;
                const data = json ? await response.json() as T : response;
                let result: FetchResult<T> = {
                    success: true,
                    errorMessage: '',
                    data: <any>data
                };

                if (ServiceBase.simulatedDelay > 0) {
                    await new Promise( resolve => setTimeout(resolve, ServiceBase.simulatedDelay) );
                }

                // Check for non 2xx codes
                if (!response.status.toString().startsWith('2'))
                {
                    result.success = false;
                    result.errorMessage = 'Something failed..';
                    result.data = null;

                    // Check raw response
                    try {
                        const responseBody = await (data as Response)?.text();
                        if (responseBody) result.errorMessage = responseBody.startsWith('"') ? JSON.parse(responseBody) : responseBody;
                    } catch {}

                    // Check for error message in response
                    try {
                        const dataAny = <any>data;
                        if (dataAny && dataAny.errorMessage)
                        {
                            result.errorMessage = dataAny.errorMessage;
                        }
                    } catch {}
                    
                    statuses.setFailed(result.errorMessage);
                    resolve(result);
                    return;
                }
                
                statuses.setSucceeded();
                resolve(result);
            } catch (err) {
                const details = JSON.stringify(err, Object.getOwnPropertyNames(err));
                const reason = err.message || "Something failed";
                statuses.setFailed(reason, details);
                reject(reason);
            }
        });
    }

    protected fetchExt(url: string, method: string, payload: any | null = null): Promise<Response>
    {
        let payloadJson = (payload == null) ? null : JSON.stringify(payload);
        return fetch(url, {
            credentials: 'include',
            method: method,
            body: payloadJson,
            headers: new Headers({
                'Content-Type': 'application/json',
                Accept: 'application/json',
            })
        })
    }
}

export interface FetchResult<TData>
{
    success: boolean;
    errorMessage: string;
    data: TData;
}

export class LoadStatus {
    public inProgress: boolean = false;
    public done: boolean = false;
    public success: boolean = false;
    public failed: boolean = false;
    public errorMessage: string | null = null;
    public errorDetails: string | null = null;
}

export class MultiLoadStatus {
    statuses: Array<LoadStatus> = [];

    constructor(statuses: Array<LoadStatus | null>) {
        this.statuses = statuses.filter(x => x != null);
    }

    setInProgress(): void { this.statuses.forEach(x => x.inProgress = true); }

    setSucceeded(): void {
        this.statuses.forEach(status => {
            status.inProgress = false;
            status.done = true;
            status.success = true;
            status.failed = false;
        });
    }

    setFailed(err: string | null, errDetails: string | null = null): void {
        this.statuses.forEach(status => {
            status.inProgress = false;
            status.done = true;
            status.failed = true;
            status.success = false;
            status.errorMessage = err;
            status.errorDetails = errDetails;
        });
    }
}
