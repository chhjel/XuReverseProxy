using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Web;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.Common;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.ProxyAuthentication.Attributes;
using XuReverseProxy.Core.Services;

namespace XuReverseProxy.Core.ProxyAuthentication.Challenges;

[GenerateFrontendModel]
public class ProxyChallengeTypeManualApproval : ProxyChallengeTypeBase
{
    public override string Name { get; } = "Manual approval";
    public CustomRequestData? RequestData { get; set; }

    public const string DataKeyRequested = "requested";
    public const string DataKeyRequestedAt = "requestedAt";
    public const string DataKeyEasyCode = "easyCode";

    public override async Task<object> CreateFrontendChallengeModelAsync(ProxyChallengeInvokeContext context)
    {
        var requestedAtRaw = await context.GetDataAsync(DataKeyRequestedAt);
        return new ProxyChallengeTypeManualApprovalFrontendModel(
            await GetEasyCodeAsync(context),
            await context.GetDataAsync(DataKeyRequested) == "true",
            requestedAtRaw == null ? default(DateTime?) : new DateTime(long.Parse(requestedAtRaw), DateTimeKind.Utc),
            context.AuthenticationData.Id
        );
    }
    [GenerateFrontendModel]
    public record ProxyChallengeTypeManualApprovalFrontendModel(string EasyCode, bool HasRequested, DateTime? LastRequestedAt, Guid AuthenticationId);

    /// <summary>
    /// Get the easy-code, create if missing.
    /// </summary>
    private async Task<string> GetEasyCodeAsync(ProxyChallengeInvokeContext context)
    {
        var code = await context.GetDataAsync(DataKeyEasyCode);
        if (code == null)
        {
            code = Guid.NewGuid().ToString()[..6].ToUpperInvariant();
            await context.SetDataAsync(DataKeyEasyCode, code);
        }
        return code;
    }

    [InvokableProxyAuthMethod]
    public async Task<RequestApprovalResponseModel> RequestApprovalAsync(ProxyChallengeInvokeContext context, RequestApprovalRequestModel _)
    {
        var hasRequested = await context.GetDataAsync(DataKeyRequested) == "true";
        if (!hasRequested)
        {
            return await RequestAsync(context);
        }

        var lastRequestedAt = new DateTime(long.Parse(await context.GetDataAsync(DataKeyRequestedAt) ?? "0"), DateTimeKind.Utc);
        if (DateTime.UtcNow - lastRequestedAt < TimeSpan.FromMinutes(5))
        {
            return new(false, "Access has been requested too recently.");
        }

        return await RequestAsync(context);
    }
    public class RequestApprovalRequestModel { }
    [GenerateFrontendModel]
    public record RequestApprovalResponseModel(bool Success, string? Error);

    private async Task<RequestApprovalResponseModel> RequestAsync(ProxyChallengeInvokeContext context)
    {
        if (string.IsNullOrWhiteSpace(RequestData?.Url)) return new RequestApprovalResponseModel(false, "WebHook not configured");

        try
        {
            var serverConfig = context.GetService<IOptionsMonitor<ServerConfig>>();
            var approvalUrl = $"{serverConfig.CurrentValue.Domain.GetFullAdminDomain()}/proxyAuth/approve/{context.ClientIdentity.Id}/{context.ProxyConfig.Id}/{context.AuthenticationData.Id}/{context.SolvedId}";
            var approvalApiUrl = $"{serverConfig.CurrentValue.Domain.GetFullAdminDomain()}/proxyAuth/approve/{context.ClientIdentity.Id}/{context.AuthenticationData.Id}/{context.SolvedId}";

            var placeholders = new Dictionary<string, string?>
            {
                { "url", approvalUrl },
                { "approvalApiUrl", approvalApiUrl }
            };
            var placeholderProviders = new IProvidesPlaceholders?[] { context.ClientIdentity, context.ProxyConfig, context.AuthenticationData };
            var placeholderResolver = context.GetService<IPlaceholderResolver>();
            await RequestData.ResolvePlaceholdersAsync(placeholderResolver, placeholders, placeholderProviders);

            var httpRequestMessage = RequestData.CreateRequest();
            if (httpRequestMessage == null) return new(false, "Webhook not configured.");

            var httpClient = context.GetService<IHttpClientFactory>().CreateClient();
            var response = await httpClient.SendAsync(httpRequestMessage);
            response.EnsureSuccessStatusCode();

            await context.SetDataAsync(DataKeyRequested, "true");
            await context.SetDataAsync(DataKeyRequestedAt, DateTime.UtcNow.Ticks.ToString());
            return new RequestApprovalResponseModel(true, null);
        }
        catch (Exception ex)
        {
            var logger = context.GetService<ILogger<ProxyChallengeTypeManualApproval>>();
            logger.LogError(ex, "Failed to send manual approval webhook to '{url}'", RequestData?.Url);
            var error =
#if DEBUG
                $"Something failed while attempting to request access. {ex.Message}";
#else
                "Something failed while attempting to request access.";
#endif
            return new(false, error);
        }
    }
}
