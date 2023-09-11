using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class AdminAuditLogEntry : IHasId
{
    public Guid Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public Guid AdminUserId { get; set; }
    public string? IP { get; set; }

    /// <summary>
    /// E.g: "[USER] created new proxy config [PROXYCONFIG]".
    /// </summary>
    public string? Action { get; set; }

    public Guid? RelatedProxyConfigId { get; set; }
    public string? RelatedProxyConfigName { get; set; }

    public Guid? RelatedClientId { get; set; }
    public string? RelatedClientName { get; set; }

    public const string Placeholder_User = "[USER]";
    public const string Placeholder_ProxyConfig = "[PROXYCONFIG]";
    public const string Placeholder_Client = "[CLIENT]";
}
