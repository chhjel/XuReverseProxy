namespace XuReverseProxy.Core.Models.Config;

/// <summary>
/// Static config values from appsettings.json
/// </summary>
public class ServerConfig
{
    public required string Name { get; set; }

    public required SecurityConfig Security { get; set; }
    public required DomainConfig Domain { get; set; }
    public required JobsConfig Jobs { get; set; }

    public class SecurityConfig
    {
        public bool RestrictAdminToLocalhost { get; set; }
        public bool BindAdminCookieToIP { get; set; }
        public bool LimitAdminLoginToSingleSession { get; set; }
        public bool ValidateUpstreamCertificateIssues { get; set; }
    }

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

        public string GetDomain(string? subdomain, int? port = null)
        {
            var resolvedPort = port ?? Port;
            var portPart = (resolvedPort == 80 || resolvedPort == 443) ? string.Empty : $":{resolvedPort}";
            if (string.IsNullOrWhiteSpace(subdomain)) return $"{Scheme}{Domain}{portPart}";
            else return $"{Scheme}{subdomain}.{Domain}{portPart}";
        }
    }

    public class JobsConfig
    {
        public required ClientIdentityCleanupJobConfig ClientIdentityCleanupJob { get; set; }
    }

    public class ClientIdentityCleanupJobConfig
    {
        public bool Enabled { get; set; }
        public long? RemoveIfNotAccessedInMinutes { get; set; }
        public long? RemoveIfNotAttemptedAccessedInMinutes { get; set; }
        public long? RemoveIfNeverAccessedAndNotAttemptedAccessedInMinutes { get; set; }
    }
}