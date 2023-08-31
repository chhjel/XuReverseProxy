import { RuntimeServerConfigItem } from './../generated/Models/Core/RuntimeServerConfigItem';
import EFCrudServiceBase from './EFCrudServiceBase';

export default class ServerConfigService extends EFCrudServiceBase<RuntimeServerConfigItem> {
    constructor() {
        super('serverConfig');
    }
}
