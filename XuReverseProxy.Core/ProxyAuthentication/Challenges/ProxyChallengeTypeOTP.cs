using Microsoft.Extensions.Logging;
using System.Web;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.Common;
using XuReverseProxy.Core.ProxyAuthentication.Attributes;
using XuReverseProxy.Core.Services;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.ProxyAuthentication.Challenges;

[GenerateFrontendModel]
public class ProxyChallengeTypeOTP : ProxyChallengeTypeBase
{
    public override string Name { get; } = "One-time-password";
    public string? Description { get; set; }
    public CustomRequestData? RequestData { get; set; }

    private const string _dataKeyOtp = "code";
    private const string _dataKeyLastSentAt = "sentAt";

    public override async Task<object> CreateFrontendChallengeModelAsync(ProxyChallengeInvokeContext context)
    {
        var lastSentAtRaw = await context.GetDataAsync(_dataKeyLastSentAt);
        return new ProxyChallengeTypeOTPFrontendModel(
            await context.GetDataAsync(_dataKeyLastSentAt) != null,
            lastSentAtRaw == null ? default(DateTime?) : new DateTime(long.Parse(lastSentAtRaw), DateTimeKind.Utc),
            Description,
            context.AuthenticationData.Id
        );
    }
    [GenerateFrontendModel]
    public record ProxyChallengeTypeOTPFrontendModel(bool HasSentCode, DateTime? CodeSentAt, string? Description, Guid AuthenticationId);

    [InvokableProxyAuthMethod]
    public async Task<TrySendOTPResponseModel> TrySendOTPAsync(ProxyChallengeInvokeContext context, TrySendOTPRequestModel _)
    {
        if (string.IsNullOrWhiteSpace(RequestData?.Url)) return new(false, "Webhook not configured");

        var lastSentAt = new DateTime(long.Parse(await context.GetDataAsync(_dataKeyLastSentAt) ?? "0"), DateTimeKind.Utc);
        if (DateTime.UtcNow - lastSentAt < TimeSpan.FromMinutes(5)) return new(false, "Code was sent recently, wait a bit before trying again.");
        
        var code = Guid.NewGuid().ToString()[..6].ToUpper();
        await context.SetDataAsync(_dataKeyOtp, code);

        var result = await SendOTPCodeAsync(context, code);
        if (result.Success)
        {
            await context.SetDataAsync(_dataKeyLastSentAt, DateTime.UtcNow.Ticks.ToString());
        }
        return result;
    }
    public class TrySendOTPRequestModel { }
    [GenerateFrontendModel]
    public record TrySendOTPResponseModel(bool Success, string? Error);

    [InvokableProxyAuthMethod]
    public async Task<TrySolveOTPResponseModel> TrySolveOTPAsync(ProxyChallengeInvokeContext context, TrySolveOTPRequestModel request)
    {
        var correctCode = await context.GetDataAsync(_dataKeyOtp);
        var solved = request.Code?.Equals(correctCode, StringComparison.OrdinalIgnoreCase) == true;
        if (!solved) return new(false, "Invalid code.");

        // Validate code lifetime, expires after 5 min
        var lastSentAtRaw = await context.GetDataAsync(_dataKeyLastSentAt);
        var lastSentAt = lastSentAtRaw == null ? default(DateTime?) : new DateTime(long.Parse(lastSentAtRaw), DateTimeKind.Utc);
        if (DateTime.UtcNow - lastSentAt > TimeSpan.FromMinutes(5))
        {
            return new(false, "Invalid code.");
        }

        await context.SetChallengeSolvedAsync();
        return new(true, null);
    }
    [GenerateFrontendModel]
    public class TrySolveOTPRequestModel
    {
        public string? Code { get; set; }
    }
    [GenerateFrontendModel]
    public record TrySolveOTPResponseModel(bool Success, string? Error);

    private async Task<TrySendOTPResponseModel> SendOTPCodeAsync(ProxyChallengeInvokeContext context, string code)
    {
        if (string.IsNullOrWhiteSpace(RequestData?.Url)) return new TrySendOTPResponseModel(false, "WebHook not configured");
        
        var httpClient = context.GetService<IHttpClientFactory>().CreateClient();

        try
        {
            var placeholders = new Dictionary<string, string?> { { "code", code } };
            var placeholderProviders = new IProvidesPlaceholders?[] { context.ClientIdentity, context.ProxyConfig, context.AuthenticationData };
            var placeholderResolver = context.GetService<IPlaceholderResolver>();
            await RequestData.ResolvePlaceholdersAsync(placeholderResolver, placeholders, placeholderProviders);
            
            var httpRequestMessage = RequestData.CreateRequest();
            if (httpRequestMessage == null) return new(false, "Webhook not configured.");

            await httpClient.SendAsync(httpRequestMessage);
            return new(true, null);
        }
        catch (Exception ex)
        {
            var logger = context.GetService<ILogger<ProxyChallengeTypeOTP>>();
            logger.LogError(ex, "Failed to send OTP webhook to '{url}'", RequestData.Url);
            var error =
#if DEBUG
                $"Something failed while attempting to send OTP-code. {ex.Message}";
#else
                "Something failed while attempting to send OTP-code.";
#endif
            return new(false, error);
        }
    }
}
