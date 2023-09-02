
import { ProxyConfig } from "@generated/Models/Core/ProxyConfig";

export function createProxyConfigResultingProxyUrl(config: ProxyConfig, serverScheme: string, serverPort: number | null, serverDomain: string): string {
    let portPart = (!config.port || config.port == 80 || config.port == 443) ? '' : `:${config.port}`;
    if (portPart == '' && (serverPort && serverPort != 80 && serverPort != 443))
        portPart = `:${serverPort}`;
    
    if (!config.subdomain || config.subdomain.length == 0)
        return `${serverScheme}${serverDomain}${portPart}`;
    else 
        return `${serverScheme}${config.subdomain.trim().toLowerCase()}.${serverDomain}${portPart}`;
}
