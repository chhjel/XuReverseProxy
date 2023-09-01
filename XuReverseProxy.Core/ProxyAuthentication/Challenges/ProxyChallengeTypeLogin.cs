﻿using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.ProxyAuthentication.Attributes;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.ProxyAuthentication.Challenges;

[GenerateFrontendModel]
public class ProxyChallengeTypeLogin : ProxyChallengeTypeBase
{
    public string? Description { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? TOTPSecret { get; set; }

    public override Task<object> CreateFrontendChallengeModelAsync(ProxyChallengeInvokeContext context)
    {
        return Task.FromResult<object>(new ProxyChallengeTypeLoginFrontendModel(Description,
            TOTPRequired: !string.IsNullOrWhiteSpace(TOTPSecret),
            UsernameRequired: !string.IsNullOrWhiteSpace(Username),
            PasswordRequired: !string.IsNullOrWhiteSpace(Password)
        ));
    }

    [GenerateFrontendModel]
    public record ProxyChallengeTypeLoginFrontendModel(string? Description, bool TOTPRequired, bool UsernameRequired, bool PasswordRequired);

    [InvokableProxyAuthMethod]
    public async Task<VerifyLoginResponseModel> VerifyLoginAsync(ProxyChallengeInvokeContext context, VerifyLoginRequestModel request)
    {
        var solved = request.Username?.Equals(Username, StringComparison.InvariantCulture) == true
               && request.Password?.Equals(PlaceholderUtils.ResolveCommonPlaceholders(Password), StringComparison.InvariantCulture) == true;
        if (!solved) return new(false, "Wrong credentials");

        var totpRequired = !string.IsNullOrWhiteSpace(TOTPSecret);
        if (totpRequired && !TotpUtils.ValidateCode(TOTPSecret, request.TOTP))
            return new(false, "Wrong credentials");

        await context.SetChallengeSolvedAsync();
        return new(true, null);
    }
    [GenerateFrontendModel]
    public class VerifyLoginRequestModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? TOTP { get; set; }
    }
    [GenerateFrontendModel]
    public record VerifyLoginResponseModel(bool Success, string? Error);
}
