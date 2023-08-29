namespace XuReverseProxy.Core.Models.Config;

/// <summary>
/// Static config values from appsettings.json
/// </summary>
public class ServerConfig
{
    public required string Name { get; set; }
    public bool RestrictAdminToLocalhost { get; set; }

    public required DomainConfig Domain { get; set; }

    public class DomainConfig
    {
        public string? Scheme { get; set; }
        public int? Port { get; set; }
        public string? Domain { get; set; }
        public string? AdminSubdomain { get; set; }

        public string GetFullAdminDomain()
            => GetDomain(AdminSubdomain);

        public string GetFullDomain()
        {
            var portPart = (Port == 80 || Port == 443) ? string.Empty : $":{Port}";
            return $"{Scheme}{Domain}{portPart}";
        }

        public string GetDomain(string? subdomain)
        {
            var portPart = (Port == 80 || Port == 443) ? string.Empty : $":{Port}";
            if (string.IsNullOrWhiteSpace(subdomain)) return $"{Scheme}{Domain}{portPart}";
            else return $"{Scheme}{subdomain}.{Domain}{portPart}";
        }
    }
}