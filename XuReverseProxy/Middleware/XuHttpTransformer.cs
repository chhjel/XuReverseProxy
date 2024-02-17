using System.Net.Http.Headers;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Services;
using XuReverseProxy.Core.Utils;
using Yarp.ReverseProxy.Forwarder;
using ServiceCollectionExtensions = XuReverseProxy.Extensions.ServiceCollectionExtensions;

namespace XuReverseProxy.Middleware;

/// <summary>
/// Extends the default transformer with some custom logic to prevent internal cookies being forwarded to the proxy target.
/// </summary>
internal class XuHttpTransformer : HttpTransformer
{
    public static XuHttpTransformer Instance { get; } = new();
    private readonly HttpTransformer _defaultTransformer;

    public XuHttpTransformer()
    {
        _defaultTransformer = Default;
    }

    public override async ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest,
        string destinationPrefix, CancellationToken cancellationToken)
    {
        var stripSourceTraces = httpContext.Items[nameof(ProxyConfig.StripUpstreamSourceTraces)] is bool strip && strip;
        var rewriteOrigin = httpContext.Items[nameof(ProxyConfig.RewriteDownstreamOrigin)] is bool rewriteOrgn && rewriteOrgn;

        if (rewriteOrigin && !string.IsNullOrWhiteSpace(httpContext.Request.Headers.Origin))
        {
            var newHost = UrlUtils.ChangeUrlAuthorityAndPort(httpContext.Request.Headers.Origin, destinationPrefix);
            httpContext.Request.Headers.Origin = newHost;
        }

        if (stripSourceTraces)
        {
            httpContext.Request.Headers.Referer = string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(httpContext.Request.Headers.Cookie))
        {
            httpContext.Request.Headers.Cookie = RemoveInternalCookies(httpContext.Request.Headers.Cookie);
        }

        await _defaultTransformer.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);

        if (stripSourceTraces) RemoveSourceHeaders(proxyRequest.Headers);
    }

    private void RemoveSourceHeaders(HttpRequestHeaders headers)
    {
        foreach (var header in headers.ToArray())
        {
            if (header.Key.StartsWith("X-Forwarded-", StringComparison.CurrentCultureIgnoreCase))
            {
                headers.Remove(header.Key);
            }
        }
    }

    private static readonly HashSet<string> _internalCookieNames = new(new[] {
            ProxyClientIdentityService.ClientIdCookieName,
            ServiceCollectionExtensions.AuthCookieName,
            ServiceCollectionExtensions.IdentityCookieName,
            ServiceCollectionExtensions.AntiForgeryCookieName
        });
    private static string? RemoveInternalCookies(string? rawCookieHeader)
    {
        if (string.IsNullOrWhiteSpace(rawCookieHeader)) return rawCookieHeader;

        static bool shouldRemoveCookie(string rawCookie)
        {
            var eqPos = rawCookie.IndexOf('=');
            if (eqPos == -1) return false;

            var name = rawCookie[..eqPos].Trim();
            return _internalCookieNames.Contains(name);
        }

        var partsToKeep = rawCookieHeader.Split(";")
            .Where(x => !shouldRemoveCookie(x));
        return string.Join(';', partsToKeep);
    }
}