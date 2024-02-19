using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QoDL.Toolkit.Core.Models;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Web.Core.Utils;
using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Web;
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
    private const string HeaderName_ERR = "XURP-ERR";
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
        IIPBlockService ipBlockService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        INotificationService notificationService,
        IPlaceholderResolver placeholderResolver,
        IConditionChecker conditionChecker)
    {
        // Check for special cases first
        if (await TryHandleInternalRequestAsync(context, _nextMiddleware))
            return;

        var host = context.Request.Host.Host;
        var hostParts = host.Split('.');
        var subdomain = hostParts.Length >= 3 ? hostParts[0] : string.Empty;
        var port = context.Request.Host.Port;

        // Check ip block
        var rawIp = TKRequestUtils.GetIPAddress(context);
        var ipData = TKIPAddressUtils.ParseIP(rawIp, acceptLocalhostString: true);
        if (ipData?.IP != null && await ipBlockService.IsIPBlockedAsync(ipData.IP))
        {
            var html = await placeholderResolver.ResolvePlaceholdersAsync(runtimeServerConfig.IPBlockedHtml);
            await SetResponseAsync(context, html, runtimeServerConfig.IPBlockedResponseCode);
            return;
        }

        // Prevent forwarding admin interface
        if ($"{subdomain}" == $"{serverConfig.CurrentValue.Domain.AdminSubdomain}")
        {
            await HandleAdminDomainRequestAsync(context, serverConfig, applicationDbContext, runtimeServerConfig, userManager, signInManager, notificationService, ipData);
            return;
        }
        // Check killswitch
        else if (!runtimeServerConfig.EnableForwarding)
        {
            var html = await placeholderResolver.ResolvePlaceholdersAsync(runtimeServerConfig.NotFoundHtml);
            await SetResponseAsync(context, html, StatusCodes.Status404NotFound);
            return;
        }

        // Prevent forwarding if no proxy is configured for the current subdomain
        var proxyConfig = (await applicationDbContext.GetWithCacheAsync(x => x.ProxyConfigs))
            .FirstOrDefault(x =>
                x.Enabled
                && x.Subdomain == subdomain
                && (x.Port == null || x.Port == port)
            );
        if (proxyConfig == null)
        {
            var html = await placeholderResolver.ResolvePlaceholdersAsync(runtimeServerConfig.NotFoundHtml);
            await SetResponseAsync(context, html, StatusCodes.Status404NotFound);
            return;
        }

        // Resolve session data for client
        ProxyClientIdentity? clientIdentity = null;
        var authentications = (await applicationDbContext.GetWithCacheAsync(x => x.ProxyAuthenticationDatas))
            .Where(x => x.ProxyConfigId == proxyConfig.Id)
            .ToArray();
        var requiresAuthentication = authentications.Any();
        if (requiresAuthentication)
        {
            clientIdentity = await proxyClientIdentityService.GetCurrentProxyClientIdentityAsync(context);
            if (clientIdentity == null)
            {
                var html = await placeholderResolver.ResolvePlaceholdersAsync(runtimeServerConfig.NotFoundHtml);
                await SetResponseAsync(context, html, StatusCodes.Status404NotFound);
                return;
            }
        }

        // Check blocked
        if (clientIdentity?.Blocked == true)
        {
            var html = (await placeholderResolver.ResolvePlaceholdersAsync(runtimeServerConfig.ClientBlockedHtml, transformer: null, placeholders: null, clientIdentity))
                ?.Replace("{{blocked_message}}", clientIdentity.BlockedMessage, StringComparison.OrdinalIgnoreCase);
            await SetResponseAsync(context, html, runtimeServerConfig.ClientBlockedResponseCode);
            return;
        }

        // Proxy conditions
        var conditionContext = conditionChecker.CreateContext();
        if (!conditionChecker.ConditionsPassed(proxyConfig.ProxyConditions, conditionContext))
        {
            // todo: make a dedicated page
            var message = HttpUtility.HtmlEncode(proxyConfig.ConditionsNotMetMessage);
            var html = $"<!DOCTYPE html>\n<html>\n<head>\n<title>{HttpUtility.HtmlEncode(proxyConfig.ChallengeTitle)}</title>\n</head>\n<body>\n{message}\n</body>\n</html>\n";
            html = (await placeholderResolver.ResolvePlaceholdersAsync(html, transformer: null, placeholders: null, clientIdentity));
            await SetResponseAsync(context, html, statusCode: 200);
            return;
        }

        // Check cached approval, access allowed is cached for 5 seconds
        var allowedCacheKey = $"__client_allowed_{proxyConfig.Id}_{clientIdentity?.Id}";
        if (memoryCache.TryGetValue(allowedCacheKey, out _))
        {
            await ForwardRequestAsync(context, forwarder, serverConfig, proxyClientIdentityService, notificationService, proxyConfig, clientIdentity);
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
                    authChallengeFactory, serviceProvider, proxyChallengeService, conditionContext);
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
        memoryCache.Set(allowedCacheKey, true, DateTimeOffset.Now + TimeSpan.FromSeconds(5));
        await ForwardRequestAsync(context, forwarder, serverConfig, proxyClientIdentityService, notificationService, proxyConfig, clientIdentity);
    }

    private static async Task ForwardRequestAsync(HttpContext context, IHttpForwarder forwarder, 
        IOptionsMonitor<ServerConfig> serverConfig, IProxyClientIdentityService proxyClientIdentityService,
        INotificationService notificationService, ProxyConfig proxyConfig, ProxyClientIdentity? clientIdentity)
    {
        await notificationService.TryNotifyEvent(NotificationTrigger.ClientRequest,
            new Dictionary<string, string?> {
                    { "Url", context.Request.GetDisplayUrl() }
            }, clientIdentity, proxyConfig);

        if (clientIdentity != null) await proxyClientIdentityService.TryUpdateLastAccessedAtAsync(clientIdentity.Id);
        if (proxyConfig.Mode == ProxyConfigMode.Forward)
        {
            await ForwardRequestAsync(context, forwarder, proxyConfig, serverConfig.CurrentValue);
        }
        else if (proxyConfig.Mode == ProxyConfigMode.StaticHTML)
        {
            await SetResponseAsync(context, proxyConfig.StaticHTML);
        }
    }

    private async Task HandleAdminDomainRequestAsync(HttpContext context, IOptionsMonitor<ServerConfig> serverConfig, ApplicationDbContext applicationDbContext,
        RuntimeServerConfig runtimeServerConfig, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        INotificationService notificationService, TKIPData? ipData)
    {
        if (!context.Items.ContainsKey("IsAdminPage")) context.Items.Add("IsAdminPage", true);

        // Validate that admin IP has not changed since login if enabled
        ApplicationUser? adminUser = null;
        if (serverConfig.CurrentValue.Security.BindAdminCookieToIP)
        {
            (var ipChanged, adminUser) = await CheckForChangedUserIP(context, applicationDbContext, ipData, userManager, signInManager, runtimeServerConfig, notificationService);
            if (ipChanged)
            {
                context.Response.Clear();
                if (context.Request.Method == HttpMethod.Get.Method)
                {
                    context.Response.Redirect("/auth/login?err=ip_changed");
                }
                else
                {
                    context.Response.Headers[HeaderName_ERR] = "ip_changed";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }

                return;
            }
        }

        if (context.User.Identity?.IsAuthenticated == true)
        {
            await notificationService.TryNotifyEvent(NotificationTrigger.AdminRequests,
                new Dictionary<string, string?> {
                        { "Url", context.Request.GetDisplayUrl() }
                }, adminUser);
        }

        await _nextMiddleware(context);
    }

    private static async Task<(bool ipChanged, ApplicationUser? user)> CheckForChangedUserIP(HttpContext context, ApplicationDbContext applicationDbContext, TKIPData? ipData,
        UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RuntimeServerConfig serverConfig, INotificationService notificationService)
    {
        // Don't logout on manual approval page if it doesnt require admin auth
        if (!serverConfig.EnableManualApprovalPageAuthentication)
        {
            var isIgnored = context.Request.Path.ToString().StartsWith("/dist/", StringComparison.OrdinalIgnoreCase)
                || context.Request.Path.ToString().Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase)
                || context.Request.Path.ToString().StartsWith("/proxyAuth/approve/", StringComparison.OrdinalIgnoreCase);
            if (isIgnored) return (false, null);
        }

        var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return (false, null);

        if ((await applicationDbContext.Users.FirstOrDefaultAsync(x => x.Id == userId)) is not ApplicationUser user) return (false, null);
        if (ipData != null && user.LastConnectedFromIP == ipData.IP) return (false, user);

        await userManager.UpdateSecurityStampAsync(user);
        await signInManager.SignOutAsync();

        applicationDbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(context, $"Session IP changed from '{user.LastConnectedFromIP}' to '{ipData?.IP}' causing all user sessions to be terminated."));
        await applicationDbContext.SaveChangesAsync();

        await notificationService.TryNotifyEvent(NotificationTrigger.AdminSessionIPChanged, 
            new Dictionary<string, string?> {
                { "OldIP", user.LastConnectedFromIP },
                { "NewIP", ipData?.IP }
            },
            user);

        return (true, user);
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
        IProxyAuthenticationChallengeFactory authChallengeFactory, IServiceProvider serviceProvider, IProxyChallengeService proxyChallengeService,
        ConditionContext conditionContext)
    {
        var conditionsPassed = await proxyChallengeService.ChallengeRequirementPassedAsync(auth.Id, conditionContext);
        (ConditionData.ConditionType Type, int Group, string Summary, bool Passed)[]? challengeData = null;
        if (!conditionsPassed)
        {
            if (proxyConfig.ShowChallengesWithUnmetRequirements)
            {
                // Update viewmodel if enabled
                challengeData = await proxyChallengeService.GetChallengeRequirementDataAsync(auth.Id, conditionContext);
                if (!challengeData.All(c => c.Passed))
                {
                    pageModel.AuthsWithUnfulfilledConditions.Add(new(auth.ChallengeTypeId!,
                        challengeData.Select(x => new ProxyChallengePageFrontendModel.AuthCondition(x.Type, x.Group, x.Summary, x.Passed)).ToList()));
                }
            }
            return AuthCheckResult.ConditionsNotMet;
        }

        var challenge = authChallengeFactory.CreateProxyAuthenticationChallenge(auth.ChallengeTypeId, auth.ChallengeJson);

        // Auth type not found => skip
        if (challenge == null)
            return AuthCheckResult.Invalid;

        var challengeContext = new ProxyChallengeInvokeContext(context, auth, proxyConfig, clientIdentity, applicationDbContext, serviceProvider, proxyChallengeService);

        // Check if challenge is auto-solved on load
        var isAutoSolved = await challenge.AutoCheckSolvedOnLoadAsync(challengeContext);
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
            challengeData ??= await proxyChallengeService.GetChallengeRequirementDataAsync(auth.Id, conditionContext);
            pageModel.ChallengeModels.Add(
                new(auth.Id, auth.ChallengeTypeId!, auth.Order, solved, frontendModel,
                    challengeData.Select(x => new ProxyChallengePageFrontendModel.AuthCondition(x.Type, x.Group, x.Summary, x.Passed)).ToList()));
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

    private static async Task SetResponseAsync(HttpContext context, string? html, int statusCode = StatusCodes.Status200OK)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(html ?? string.Empty);
    }

    private static async Task ForwardRequestAsync(HttpContext context, IHttpForwarder forwarder, ProxyConfig proxyConfig, ServerConfig serverConfig)
    {
        var destinationPrefix = proxyConfig.DestinationPrefix;
        if (string.IsNullOrWhiteSpace(destinationPrefix)) return;

        var transformer = XuHttpTransformer.Instance;
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

        context.Items[nameof(ProxyConfig.StripUpstreamSourceTraces)] = proxyConfig.StripUpstreamSourceTraces;
        context.Items[nameof(ProxyConfig.RewriteDownstreamOrigin)] = proxyConfig.RewriteDownstreamOrigin;

        var httpClient = new HttpMessageInvoker(socksHandler);
        var error = await forwarder.SendAsync(context, destinationPrefix, httpClient, requestOptions, transformer);
        if (error != ForwarderError.None)
        {
            // todo: handle?
        }
    }
}
