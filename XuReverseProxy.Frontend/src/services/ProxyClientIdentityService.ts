import { ProxyClientIdentity } from './../generated/Models/Core/ProxyClientIdentity';
import EFCrudServiceBase from './EFCrudServiceBase';


export default class ProxyClientIdentityService extends EFCrudServiceBase<ProxyClientIdentity> {
    constructor() {
        super('proxyClientIdentity');
    }
}
