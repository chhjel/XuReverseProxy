using Microsoft.AspNetCore.Identity;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.ProxyAuthentication.Attributes;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.ProxyAuthentication.Challenges;

[GenerateFrontendModel]
public class ProxyChallengeTypeAdminLogin : ProxyChallengeTypeBase
{
    public string? Description { get; set; }

    public override Task<object> CreateFrontendChallengeModelAsync(ProxyChallengeInvokeContext context)
    {
        return Task.FromResult<object>(new ProxyChallengeTypeAdminLoginFrontendModel(Description, context.AuthenticationData.Id));
    }

    [GenerateFrontendModel]
    public record ProxyChallengeTypeAdminLoginFrontendModel(string? Description, Guid AuthenticationId);

    [InvokableProxyAuthMethod]
    public async Task<VerifyAdminLoginResponseModel> VerifyLoginAsync(ProxyChallengeInvokeContext context, VerifyAdminLoginRequestModel request)
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

        await context.SetChallengeSolvedAsync();
        return new(true, null);
    }
    [GenerateFrontendModel]
    public class VerifyAdminLoginRequestModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? TOTP { get; set; }
    }
    [GenerateFrontendModel]
    public record VerifyAdminLoginResponseModel(bool Success, string? Error);
}
