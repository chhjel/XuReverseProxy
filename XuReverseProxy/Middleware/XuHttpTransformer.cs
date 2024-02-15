using XuReverseProxy.Core.Services;
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
        if (!string.IsNullOrWhiteSpace(httpContext.Request.Headers.Cookie))
        {
            httpContext.Request.Headers.Cookie = RemoveInternalCookies(httpContext.Request.Headers.Cookie);
        }
        await _defaultTransformer.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix, cancellationToken);
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