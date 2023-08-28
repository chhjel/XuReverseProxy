using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
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
        IProxyConfigService proxyConfigService, IOptionsMonitor<ServerConfig> serverConfig,
        IProxyAuthenticationChallengeFactory authChallengeFactory,
        IProxyClientIdentityService proxyClientIdentityService,
        ApplicationDbContext applicationDbContext,
        IServiceProvider serviceProvider,
        RuntimeServerConfig runtimeServerConfig,
        IProxyAuthenticationConditionChecker proxyAuthenticationConditionChecker)
    {
        var host = context.Request.Host.Host;
        var hostParts = host.Split('.');
        var subdomain = hostParts.Length >= 3 ? hostParts[0] : null;
        var port = context.Request.Host.Port;

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
        var proxyConfig = await proxyConfigService.GetProxyConfigAsync(subdomain, port);
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
                var html = PlaceholderUtils.ResolvePlaceholders(runtimeServerConfig.NotFoundHtml, clientIdentity);
                await SetResponse(context, html, StatusCodes.Status404NotFound);
                return;
            }
        }

        if (clientIdentity?.Blocked == true)
        {
            var html = PlaceholderUtils.ResolvePlaceholders(runtimeServerConfig.ClientBlockedHtml, clientIdentity);
            await SetResponse(context, html);
            return;
        }

        // Process authentications if any
        if (requiresAuthentication && clientIdentity != null)
        {
            var handled = await TryHandleProxyAuthAPIAsync(context, authChallengeFactory, proxyClientIdentityService, applicationDbContext, serviceProvider, proxyConfig, clientIdentity);
            if (handled) return;

            var allChallengesSolved = true;
            var pageModel = new ProxyChallengePageFrontendModel()
            {
                Title = proxyConfig.ChallengeTitle ?? string.Empty,
                Description = proxyConfig.ChallengeDescription
            };
            foreach (var auth in authentications)
            {
                var solved = await ProcessAuthenticationCheckAsync(auth, clientIdentity, proxyConfig, context, pageModel, applicationDbContext,
                    proxyAuthenticationConditionChecker, authChallengeFactory, proxyClientIdentityService, serviceProvider);
                if (!solved) allChallengesSolved = false;
            }
            pageModel.ChallengeModels = pageModel.ChallengeModels.OrderBy(x => x.Order).ToList();

            // Show challenge page if everything isnt solved yet
            if (!allChallengesSolved)
            {
                await ShowAuthChallengeAsync(context, pageModel, serverConfig.CurrentValue);
                return;
            }
        }

        await forwardRequest(context, forwarder, proxyClientIdentityService, proxyConfig, clientIdentity);
        static async Task forwardRequest(HttpContext context, IHttpForwarder forwarder, IProxyClientIdentityService proxyClientIdentityService, ProxyConfig proxyConfig, ProxyClientIdentity? clientIdentity)
        {
            if (clientIdentity != null) await proxyClientIdentityService.TryUpdateLastAccessedAtAsync(clientIdentity.Id);
            await ForwardRequestAsync(context, forwarder, proxyConfig);
        }
    }

    private static async Task<bool> ProcessAuthenticationCheckAsync(ProxyAuthenticationData auth, ProxyClientIdentity clientIdentity,
        ProxyConfig proxyConfig, HttpContext context, ProxyChallengePageFrontendModel pageModel, ApplicationDbContext applicationDbContext,
        IProxyAuthenticationConditionChecker proxyAuthenticationConditionChecker, IProxyAuthenticationChallengeFactory authChallengeFactory,
        IProxyClientIdentityService proxyClientIdentityService, IServiceProvider serviceProvider)
    {
        var conditions = applicationDbContext.ProxyAuthenticationConditions.Where(x => x.AuthenticationDataId == auth.Id);
        var conditionResults = conditions.AsEnumerable().Select(c => new
        {
            Type = c.ConditionType,
            Summary = c.CreateSummary(),
            Passed = proxyAuthenticationConditionChecker.ConditionPassed(c)
        }).ToArray();
        if (!conditionResults.All(c => c.Passed) && proxyConfig.ShowChallengesWithUnmetRequirements)
        {
            // Update viewmodel
            pageModel.AuthsWithUnfulfilledConditions.Add(new(auth.ChallengeTypeId!,
                conditionResults.Select(x => new ProxyChallengePageFrontendModel.AuthCondition(x.Type, x.Summary, x.Passed)).ToList()));
            return false;
        }

        var challenge = authChallengeFactory.CreateProxyAuthenticationChallenge(auth.ChallengeTypeId, auth.ChallengeJson);

        // Auth type not found => skip
        if (challenge == null)
            return false;

        var challengeContext = new ProxyChallengeInvokeContext(context, auth, proxyConfig, clientIdentity,
            proxyClientIdentityService, applicationDbContext, serviceProvider);

        // Check if challenge is auto-solved on load
        var isAutoSolved = challenge.AutoCheckSolvedOnLoad(challengeContext);
        if (isAutoSolved)
        {
            await proxyClientIdentityService.SetChallengeSolvedAsync(clientIdentity.Id, auth.Id, auth.SolvedId);
        }

        // Create challenge data for frontend
        var frontendModel = await challenge.CreateFrontendChallengeModelAsync(challengeContext);
        var solved = isAutoSolved || await proxyClientIdentityService.IsChallengeSolvedAsync(clientIdentity.Id, auth);

        // Update viewmodel
        if (proxyConfig.ShowCompletedChallenges || !solved)
        {
            pageModel.ChallengeModels.Add(
                new(auth.Id, auth.ChallengeTypeId!, auth.Order, solved, frontendModel,
                    conditionResults.Select(x => new ProxyChallengePageFrontendModel.AuthCondition(x.Type, x.Summary, x.Passed)).ToList()));
        }

        return solved;
    }

    #region Proxy auth challenge api
    private static async Task<bool> TryHandleProxyAuthAPIAsync(HttpContext context, IProxyAuthenticationChallengeFactory authChallengeFactory, IProxyClientIdentityService proxyClientIdentityService, ApplicationDbContext applicationDbContext, IServiceProvider serviceProvider, ProxyConfig? proxyConfig, ProxyClientIdentity? clientIdentity)
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

        var auth = proxyConfig?.Authentications?.FirstOrDefault(x => x.ChallengeTypeId == challengeTypeId);
        if (auth == null) return false;

        var challenge = authChallengeFactory.CreateProxyAuthenticationChallenge(challengeTypeId, auth.ChallengeJson);
        if (challenge == null) return false;

#if DEBUG // delay a bit to test frontend
        await Task.Delay(500);
#endif

        var jsonPayload = await context.Request.ReadBodyAsStringAsync();
        var challengeContext = new ProxyChallengeInvokeContext(context, auth, proxyConfig!, clientIdentity,
            proxyClientIdentityService, applicationDbContext, serviceProvider);
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
    private static async Task ShowAuthChallengeAsync(HttpContext context, ProxyChallengePageFrontendModel pageModel, ServerConfig serverConfig)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        string html = CreateChallengePageHtml(serverConfig, pageModel);
        await context.Response.WriteAsync(html);
    }

    private static string CreateChallengePageHtml(ServerConfig config, ProxyChallengePageFrontendModel pageModel)
    {
        // todo: hide admin domain for js/css in frontend. Intercept and handle instead.
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
    <link rel=""stylesheet"" href=""{config.Domain.GetFullAdminDomain()}/dist/serverui.css"" asp-append-version=""true"" />
</head>
<body>
    <main role=""main"">
        <div data-vue-component=""ProxyChallengePage"" data-vue-options=""{WebUtility.HtmlEncode(pageModelJson)}""></div>
    </main>

    <script defer src=""{config.Domain.GetFullAdminDomain()}/dist/serverui.js"" asp-append-version=""true""></script>
</body>
</html>";
    }
    #endregion

    private static async Task SetResponse(HttpContext context, string html, int statusCode = StatusCodes.Status200OK)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(html);
    }

    private static async Task ForwardRequestAsync(HttpContext context, IHttpForwarder forwarder, ProxyConfig proxyConfig)
    {
        var destinationPrefix = proxyConfig.DestinationPrefix;
        if (string.IsNullOrWhiteSpace(destinationPrefix)) return;

        var transformer = HttpTransformer.Default;
        var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
        var httpClient = new HttpMessageInvoker(new SocketsHttpHandler()
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,
            ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current)
        });

        var error = await forwarder.SendAsync(context, destinationPrefix, httpClient, requestOptions, transformer);
        if (error != ForwarderError.None)
        {
            // todo log. Add LastErrorAt & update if more than 5 min since last?
        }
    }
}
