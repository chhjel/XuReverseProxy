using Microsoft.AspNetCore.Identity;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.ProxyAuthentication.Attributes;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.ProxyAuthentication.Challenges;

[GenerateFrontendModel]
public class ProxyChallengeTypeLogin : ProxyChallengeTypeBase
{
    public string? Description { get; set; }
    public bool UseIdentity { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? TOTPSecret { get; set; }

    public override Task<object> CreateFrontendChallengeModelAsync(ProxyChallengeInvokeContext context)
    {
        var totpRequired = !string.IsNullOrWhiteSpace(TOTPSecret);
        return Task.FromResult<object>(new ProxyChallengeTypeLoginFrontendModel(UseIdentity, totpRequired, Description));
    }

    [GenerateFrontendModel]
    public record ProxyChallengeTypeLoginFrontendModel(bool IsIdentity, bool TOTPRequired, string? Description);

    [InvokableProxyAuthMethod]
    public async Task<VerifyLoginResponseModel> VerifyLoginAsync(ProxyChallengeInvokeContext context, VerifyLoginRequestModel request)
    {
        if (UseIdentity)
        {
            if (request.Username == null || request.Password == null) return new(false, "Wrong credentials");

            var userManager = context.GetService<UserManager<ApplicationUser>>();
            var signInManager = context.GetService<SignInManager<ApplicationUser>>();
            var user = await userManager.FindByNameAsync(request.Username);
            if (user == null) return new(false, "Wrong credentials");

            var solved = (await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false)).Succeeded;
            if (!solved) return new(false, "Wrong credentials");

            if (user.TOTPEnabled && !TotpUtils.ValidateCode(user.TOTPSecretKey, request.TOTP))
                return new(false, "Wrong credentials");
        }
        else
        {
            var solved = request.Username?.Equals(Username, StringComparison.InvariantCulture) == true
                   && request.Password?.Equals(PlaceholderUtils.ResolveCommonPlaceholders(Password), StringComparison.InvariantCulture) == true;
            if (!solved) return new(false, "Wrong credentials");
            
            var totpRequired = !string.IsNullOrWhiteSpace(TOTPSecret);
            if (totpRequired && !TotpUtils.ValidateCode(TOTPSecret, request.TOTP))
                return new(false, "Wrong credentials");
        }

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
