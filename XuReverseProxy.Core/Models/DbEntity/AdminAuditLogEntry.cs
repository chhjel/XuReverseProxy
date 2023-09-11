using Microsoft.AspNetCore.Http;
using QoDL.Toolkit.Web.Core.Utils;
using System.Security.Claims;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class AdminAuditLogEntry : IHasId
{
    public Guid Id { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public Guid AdminUserId { get; set; }
    public string? IP { get; set; }

    /// <summary>
    /// E.g: "Created new proxy config [PROXYCONFIG]".
    /// </summary>
    public string? Action { get; set; }

    public Guid? RelatedProxyConfigId { get; set; }
    public string? RelatedProxyConfigName { get; set; }

    public Guid? RelatedClientId { get; set; }
    public string? RelatedClientName { get; set; }

    public AdminAuditLogEntry()
    {
        TimestampUtc = DateTime.UtcNow;
    }

    public AdminAuditLogEntry(HttpContext? context, Guid userId, string action) : this()
    {
        AdminUserId = userId;
        IP = TKRequestUtils.GetIPAddress(context!);
        Action = action;
    }

    public AdminAuditLogEntry(HttpContext? context, string action) : this()
    {
        var userId = context?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId != null && Guid.TryParse(userId, out var parsedUserId))
        {
            AdminUserId = parsedUserId;
        }
        IP = TKRequestUtils.GetIPAddress(context!);
        Action = action;
    }

    public AdminAuditLogEntry SetRelatedClient(Guid? relatedClientId, string? relatedClientName)
    {
        RelatedClientId = relatedClientId;
        RelatedClientName = relatedClientName;
        return this;
    }

    public AdminAuditLogEntry SetRelatedProxyConfig(Guid? relatedProxyConfigId, string? relatedProxyConfigName)
    {
        RelatedProxyConfigId = relatedProxyConfigId;
        RelatedProxyConfigName = relatedProxyConfigName;
        return this;
    }

    public const string Placeholder_User = "[USER]";
    public const string Placeholder_ProxyConfig = "[PROXYCONFIG]";
    public const string Placeholder_Client = "[CLIENT]";
}
