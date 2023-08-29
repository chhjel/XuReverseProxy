using Microsoft.Extensions.Options;
using System.Web;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.ProxyAuthentication.Attributes;

namespace XuReverseProxy.Core.ProxyAuthentication.Challenges;

public class ProxyChallengeTypeManualApproval : ProxyChallengeTypeBase
{
    public string? WebHookUrl { get; set; }
    public string? WebHookRequestMethod { get; set; }

    public const string DataKeyRequested = "requested";
    public const string DataKeyRequestedAt = "requestedAt";
    public const string DataKeyEasyCode = "easyCode";

    public override async Task<object> CreateFrontendChallengeModelAsync(ProxyChallengeInvokeContext context)
    {
        var requestedAtRaw = await context.GetDataAsync(DataKeyRequestedAt);
        return new ProxyChallengeTypeManualApprovalFrontendModel(
            await GetEasyCodeAsync(context),
            await context.GetDataAsync(DataKeyRequested) == "true",
            requestedAtRaw == null ? default(DateTime?) : new DateTime(long.Parse(requestedAtRaw), DateTimeKind.Utc)
        );
    }
    [GenerateFrontendModel]
    public record ProxyChallengeTypeManualApprovalFrontendModel(string EasyCode, bool HasRequested, DateTime? LastRequestedAt);

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
        if (string.IsNullOrWhiteSpace(WebHookUrl)) return new RequestApprovalResponseModel(false, "WebHook not configured");

        try
        {
            var serverConfig = context.GetService<IOptionsMonitor<ServerConfig>>();
            var approvalUrl = $"{serverConfig.CurrentValue.Domain.GetFullAdminDomain()}/proxyAuth/approve/{context.ClientIdentity.Id}/{context.ProxyConfig.Id}/{context.AuthenticationData.Id}/{context.SolvedId}";

            var httpClient = context.GetService<IHttpClientFactory>().CreateClient();
            var url = WebHookUrl?.Replace("{{url}}", HttpUtility.UrlEncode(approvalUrl));

            var method = HttpMethod.Get;
            if (!string.IsNullOrWhiteSpace(WebHookRequestMethod)) method = new HttpMethod(WebHookRequestMethod);

            var httpRequestMessage = new HttpRequestMessage(method, url);
            await httpClient.SendAsync(httpRequestMessage);

            await context.SetDataAsync(DataKeyRequested, "true");
            await context.SetDataAsync(DataKeyRequestedAt, DateTime.UtcNow.Ticks.ToString());
            return new RequestApprovalResponseModel(true, null);
        }
        catch(Exception ex)
        {
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
