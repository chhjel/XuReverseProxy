using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Extensions;

public static class ProxyClientIdentityExtensions
{
    public static string NameForLog(this ProxyClientIdentity identity)
        => !string.IsNullOrWhiteSpace(identity?.Note) ? identity.Note : identity?.IP ?? "Unknown";
}
