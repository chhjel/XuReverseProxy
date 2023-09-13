using Microsoft.AspNetCore.Http;
using QoDL.Toolkit.Web.Core.Utils;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class ClientAuditLogEntry : IHasId
{
    public Guid Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public Guid ClientId { get; set; }
    public string? IP { get; set; }

    /// <summary>
    /// E.g: "Completed login challenge for [Test 3 config]".
    /// </summary>
    public string? Action { get; set; }

    public Guid? RelatedProxyConfigId { get; set; }
    public string? RelatedProxyConfigName { get; set; }

    public const string Placeholder_ProxyConfig = "[PROXYCONFIG]";

    public ClientAuditLogEntry()
    {
        TimestampUtc = DateTime.UtcNow;
    }

    public ClientAuditLogEntry(HttpContext? context, Guid clientId, string action) : this()
    {
        ClientId = clientId;
        IP = TKRequestUtils.GetIPAddress(context!);
        Action = action;
    }

    public ClientAuditLogEntry SetRelatedProxyConfig(Guid? relatedProxyConfigId, string? relatedProxyConfigName)
    {
        RelatedProxyConfigId = relatedProxyConfigId;
        RelatedProxyConfigName = relatedProxyConfigName;
        return this;
    }
}
