using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Web.Core.Utils;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using XuReverseProxy.Core.Extensions;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.ProxyAuthentication;
using XuReverseProxy.Core.Services;
using XuReverseProxy.Core.Utils;
using XuReverseProxy.Models.ViewModels.Special;
using Yarp.ReverseProxy.Forwarder;

namespace XuReverseProxy.Middleware;

/// <summary>
/// Handles core logic, deciding where to send each request, when to show auth challenges etc.
/// </summary>
public class ReverseProxyMiddleware
{
    private readonly RequestDelegate _nextMiddleware;

    public ReverseProxyMiddleware(RequestDelegate nextMiddleware)
    {
        _nextMiddleware = nextMiddleware;
    }

    public async Task Invoke(HttpContext context, IHttpForwarder forwarder,
        IOptionsMonitor<ServerConfig> serverConfig,
        IProxyAuthenticationChallengeFactory authChallengeFactory,
        IProxyClientIdentityService proxyClientIdentityService,
        ApplicationDbContext applicationDbContext,
        IServiceProvider serviceProvider,
        RuntimeServerConfig runtimeServerConfig,
        IProxyChallengeService proxyChallengeService,
        IMemoryCache memoryCache,
        IIPBlockService ipBlockService)
    {
        // Check for special cases first
        if (await TryHandleInternalRequestAsync(context, _nextMiddleware))
            return;

        var host = context.Request.Host.Host;
        var hostParts = host.Split('.');
        var subdomain = hostParts.Length >= 3 ? hostParts[0] : string.Empty;
        var port = context.Request.Host.Port;

        var rawIp = TKRequestUtils.GetIPAddress(context);
        var ipData = TKIPAddressUtils.ParseIP(rawIp, acceptLocalhostString: true);
        if (ipData?.IP != null && await ipBlockService.IsIPBlockedAsync(ipData.IP))
        {
            var html = PlaceholderUtils.ResolvePlaceholders(runtimeServerConfig.IPBlockedHtml);
            await SetResponse(context, html, runtimeServerConfig.IPBlockedResponseCode);
            return;
        }

        // Prevent forwarding admin interface
        if ($"{subdomain}" == $"{serverConfig.CurrentValue.Domain.AdminSubdomain}")
        {
            if (!context.Items.ContainsKey("IsAdminPage")) context.Items.Add("IsAdminPage", true);
            await _nextMiddleware(context);
            return;
        }
        // Check killswitch
        else if (!runtimeServerConfig.EnableForwarding)
        {
            var html = PlaceholderUtils.ResolvePlaceholders(runtimeServerConfig.NotFoundHtml);
            await SetResponse(context, html, StatusCodes.Status404NotFound);
            return;
        }

        // Prevent forwarding if no proxy is configured for the current subdomain
        var proxyConfig = await applicationDbContext.ProxyConfigs.FirstOrDefaultAsync(x =>
            x.Enabled
            && x.Subdomain == subdomain
            && (x.Port == null || x.Port == port)
        );
        if (proxyConfig == null)
        {
            var html = PlaceholderUtils.ResolvePlaceholders(runtimeServerConfig.NotFoundHtml);
            await SetResponse(context, html, StatusCodes.Status404NotFound);
            return;
        }

        // Resolve session data for client
        ProxyClientIdentity? clientIdentity = null;
        var authentications = applicationDbContext.ProxyAuthenticationDatas.Where(x => x.ProxyConfigId == proxyConfig.Id).ToArray();
        var requiresAuthentication = authentications.Any();
        if (requiresAuthentication)
        {
            clientIdentity = await proxyClientIdentityService.GetCurrentProxyClientIdentityAsync(context);
            if (clientIdentity == null)
            {
                var html = PlaceholderUtils.ResolvePlaceholders(runtimeServerConfig.NotFoundHtml);
                await SetResponse(context, html, StatusCodes.Status404NotFound);
                return;
            }
        }

        // Check blocked
        if (clientIdentity?.Blocked == true)
        {
            var html = PlaceholderUtils.ResolvePlaceholders(runtimeServerConfig.ClientBlockedHtml, clientIdentity)
                ?.Replace("{{blocked_message}}", clientIdentity.BlockedMessage, StringComparison.OrdinalIgnoreCase);
            await SetResponse(context, html, runtimeServerConfig.ClientBlockedResponseCode);
            return;
        }

        // Check cached approval, access allowed is cached for 5 seconds
        if (memoryCache.TryGetValue($"__client_allowed_{proxyConfig.Id}", out _))
        {
            await forwardRequest();
            return;
        }

        // Process authentications if any
        if (requiresAuthentication && clientIdentity != null)
        {
            var handled = await TryHandleProxyAuthAPIAsync(context, authChallengeFactory, proxyClientIdentityService, applicationDbContext,
                serviceProvider, proxyConfig, clientIdentity, proxyChallengeService);
            if (handled) return;

            var allChallengesSolved = true;
            var pageModel = new ProxyChallengePageFrontendModel()
            {
                Title = proxyConfig.ChallengeTitle ?? string.Empty,
                Description = proxyConfig.ChallengeDescription
            };
            foreach (var auth in authentications)
            {
                var authResult = await ProcessAuthenticationCheckAsync(auth, clientIdentity, proxyConfig, context, pageModel, applicationDbContext,
                    authChallengeFactory, serviceProvider, proxyChallengeService);
                if (authResult == AuthCheckResult.NotSolved) allChallengesSolved = false;
            }
            pageModel.ChallengeModels = pageModel.ChallengeModels.OrderBy(x => x.Order).ToList();

            // Show challenge page if everything isnt solved yet
            if (!allChallengesSolved)
            {
                await ShowAuthChallengeAsync(context, pageModel);
                return;
            }
        }

        // Allowed => update cache & forward
        memoryCache.Set($"__client_allowed_{proxyConfig.Id}", true, DateTimeOffset.Now + TimeSpan.FromSeconds(5));
        await forwardRequest();

        async Task forwardRequest()
        {
            if (clientIdentity != null) await proxyClientIdentityService.TryUpdateLastAccessedAtAsync(clientIdentity.Id);
            await ForwardRequestAsync(context, forwarder, proxyConfig, serverConfig.CurrentValue);
        }
    }

    private enum AuthCheckResult
    {
        NotSolved = 0,
        Solved,
        ConditionsNotMet,
        Invalid
    }
    private static async Task<AuthCheckResult> ProcessAuthenticationCheckAsync(ProxyAuthenticationData auth, ProxyClientIdentity clientIdentity,
        ProxyConfig proxyConfig, HttpContext context, ProxyChallengePageFrontendModel pageModel, ApplicationDbContext applicationDbContext,
        IProxyAuthenticationChallengeFactory authChallengeFactory, IServiceProvider serviceProvider, IProxyChallengeService proxyChallengeService)
    {
        var challengeData = proxyChallengeService.GetChallengeRequirementData(auth.Id);
        if (!challengeData.All(c => c.Passed))
        {
            // Update viewmodel
            if (proxyConfig.ShowChallengesWithUnmetRequirements)
            {
                pageModel.AuthsWithUnfulfilledConditions.Add(new(auth.ChallengeTypeId!,
                    challengeData.Select(x => new ProxyChallengePageFrontendModel.AuthCondition(x.Type, x.Summary, x.Passed)).ToList()));
            }
            return AuthCheckResult.ConditionsNotMet;
        }

        var challenge = authChallengeFactory.CreateProxyAuthenticationChallenge(auth.ChallengeTypeId, auth.ChallengeJson);

        // Auth type not found => skip
        if (challenge == null)
            return AuthCheckResult.Invalid;

        var challengeContext = new ProxyChallengeInvokeContext(context, auth, proxyConfig, clientIdentity, applicationDbContext, serviceProvider, proxyChallengeService);

        // Check if challenge is auto-solved on load
        var isAutoSolved = challenge.AutoCheckSolvedOnLoad(challengeContext);
        if (isAutoSolved)
        {
            await proxyChallengeService.SetChallengeSolvedAsync(clientIdentity.Id, auth.Id, auth.SolvedId);
        }

        // Create challenge data for frontend
        var frontendModel = await challenge.CreateFrontendChallengeModelAsync(challengeContext);
        var solved = isAutoSolved || await proxyChallengeService.IsChallengeSolvedAsync(clientIdentity.Id, auth);

        // Update viewmodel
        if (proxyConfig.ShowCompletedChallenges || !solved)
        {
            pageModel.ChallengeModels.Add(
                new(auth.Id, auth.ChallengeTypeId!, auth.Order, solved, frontendModel,
                    challengeData.Select(x => new ProxyChallengePageFrontendModel.AuthCondition(x.Type, x.Summary, x.Passed)).ToList()));
        }

        return solved ? AuthCheckResult.Solved : AuthCheckResult.NotSolved;
    }

    #region Proxy auth challenge api
    private static async Task<bool> TryHandleProxyAuthAPIAsync(HttpContext context, IProxyAuthenticationChallengeFactory authChallengeFactory,
        IProxyClientIdentityService proxyClientIdentityService, ApplicationDbContext applicationDbContext, IServiceProvider serviceProvider,
        ProxyConfig? proxyConfig, ProxyClientIdentity? clientIdentity, IProxyChallengeService proxyChallengeService)
    {
        if (proxyConfig == null || clientIdentity == null) return false;

        // /proxyAuth/api/<authTypeName>/<methodName>
        const string proxyAuthApiPathPrefix = "/proxyAuth/api/";
        if (context.Request.Method != "POST" || !context.Request.Path.ToString().StartsWith(proxyAuthApiPathPrefix))
            return false;

        var pathSegments = context.Request.Path.ToString()[proxyAuthApiPathPrefix.Length..].Split('/');
        if (pathSegments.Length < 2) return false;
        var challengeTypeId = pathSegments[0];
        var methodName = pathSegments[1];
        if (!Guid.TryParse(pathSegments[2], out var authId)) return false;

        var auth = proxyConfig?.Authentications?.FirstOrDefault(x => x.Id == authId && x.ChallengeTypeId == challengeTypeId);
        if (auth == null) return false;

        var challenge = authChallengeFactory.CreateProxyAuthenticationChallenge(challengeTypeId, auth.ChallengeJson);
        if (challenge == null) return false;

#if DEBUG // delay a bit to test frontend
        await Task.Delay(500);
#endif

        var jsonPayload = await context.Request.ReadBodyAsStringAsync();
        var challengeContext = new ProxyChallengeInvokeContext(context, auth, proxyConfig!, clientIdentity, applicationDbContext, serviceProvider, proxyChallengeService);
        var result = await InvokableProxyAuthMethodUtils.InvokeMethodAsync(challenge, methodName, jsonPayload, challengeContext);
        await ReturnJsonAsync(context, result);

        return true;
    }

    private static async Task ReturnJsonAsync(HttpContext context, object? data)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        var json = data == null ? string.Empty : JsonSerializer.Serialize(data, JsonConfig.DefaultOptions);
        await context.Response.WriteAsync(json);
    }
    #endregion

    #region Proxy auth challenge view
    private static async Task ShowAuthChallengeAsync(HttpContext context, ProxyChallengePageFrontendModel pageModel)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        string html = CreateChallengePageHtml(pageModel);
        await context.Response.WriteAsync(html);
    }

    private static string CreateChallengePageHtml(ProxyChallengePageFrontendModel pageModel)
    {
        var title = !string.IsNullOrWhiteSpace(pageModel.Title) ? pageModel.Title : "XuReverseProxy";
        var pageModelJson = JsonSerializer.Serialize(pageModel, JsonConfig.DefaultOptions);
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <meta name=""robots"" content=""none"" />
    <title>{title}</title>
    <link rel=""stylesheet"" href=""https://fonts.googleapis.com/css?family=Montserrat|Material+Icons"" crossorigin=""anonymous"" />
    <link rel=""stylesheet"" href=""{CreateInternalUrl("dist/serverui.css")}"" asp-append-version=""true"" />
</head>
<body>
    <main role=""main"">
        <div data-vue-component=""ProxyChallengePage"" data-vue-options=""{WebUtility.HtmlEncode(pageModelJson)}""></div>
    </main>

    <script defer src=""{CreateInternalUrl("dist/serverui.js")}"" asp-append-version=""true""></script>
</body>
</html>";
    }
    #endregion

    private static readonly Guid _internalEndpointGuid = Guid.NewGuid();
    private static string CreateInternalUrl(string path) => $"/proxy-internal/{_internalEndpointGuid}/{path}";

    private static async Task<bool> TryHandleInternalRequestAsync(HttpContext context, RequestDelegate _nextMiddleware)
    {
        // Proxying assets through here to not expose admin subdomain on e.g. the challenge page.
        var distPrefix = CreateInternalUrl("dist/");
        if (context.Request.Method == "GET" && context.Request.Path.ToString().StartsWith(distPrefix))
        {
            context.Request.Path = $"/dist/{context.Request.Path.Value?[distPrefix.Length..]}";
            await _nextMiddleware(context);
            return true;
        }

        return false;
    }

    private static async Task SetResponse(HttpContext context, string? html, int statusCode = StatusCodes.Status200OK)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(html ?? string.Empty);
    }

    private static async Task ForwardRequestAsync(HttpContext context, IHttpForwarder forwarder, ProxyConfig proxyConfig, ServerConfig serverConfig)
    {
        var destinationPrefix = proxyConfig.DestinationPrefix;
        if (string.IsNullOrWhiteSpace(destinationPrefix)) return;

        var transformer = XuHttpTransformer.Instance; //HttpTransformer.Default;
        var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

        var socksHandler = new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current)
        };
        if (!serverConfig.Security.ValidateUpstreamCertificateIssues)
        {
            socksHandler.SslOptions = new()
            {
                RemoteCertificateValidationCallback = (object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true
            };
        }
        var httpClient = new HttpMessageInvoker(socksHandler);

        var error = await forwarder.SendAsync(context, destinationPrefix, httpClient, requestOptions, transformer);
        if (error != ForwarderError.None)
        {
            // todo log. Add LastErrorAt & update if more than 5 min since last?
        }
    }
}
