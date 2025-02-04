using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Web.Core.Utils;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Services;
using XuReverseProxy.Core.Utils;
using XuReverseProxy.Models.ViewModels.Pages;

namespace XuReverseProxy.Controllers.Pages;

[Route("auth/login/[action]")]
public class LoginPageController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IOptionsMonitor<ServerConfig> serverConfig,
    ApplicationDbContext dbContext,
    INotificationService notificationService) : Controller
{
    [HttpGet("/auth/login")]
    public IActionResult Index([FromQuery] string? @return = null, [FromQuery] string? e = null)
    {
        ViewBag.PageTitle = "Login";
        if (@return?.StartsWith("/") != true) @return = "/";

        // if already logged in => redirect to return url
        if (User?.Identity?.IsAuthenticated == true)
        {
            return Redirect(@return);
        }

        // Keep it simple for now. If no users exist, allow creating an admin account.
        var allowCreateAdmin = false;
        if (!userManager.Users.Any())
        {
            allowCreateAdmin = true;
        }

        var model = new LoginPageViewModel
        {
            FrontendModel = new LoginPageViewModel.LoginPageFrontendModel
            {
                ServerName = serverConfig.CurrentValue.Name,
                ReturnUrl = @return,
                ErrorCode = e,
                IsRestrictedToLocalhost =
                    serverConfig.CurrentValue.Security.RestrictAdminToLocalhost &&
                    !TKRequestUtils.IsLocalRequest(Request.HttpContext),
                AllowCreateAdmin = allowCreateAdmin,
                FreshTotpSecret = allowCreateAdmin ? TotpUtils.GenerateNewKey() : null
            }
        };
        return View(model);
    }

    [HttpPost("/auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Delay a bit to make timing attacks harder
        await AuthUtils.RandomAuthDelay();

        if (!ModelState.IsValid) return BadRequest();
        else if (serverConfig.CurrentValue.Security.RestrictAdminToLocalhost &&
                 !TKRequestUtils.IsLocalRequest(Request.HttpContext)) return createResult(false);

        IActionResult createResult(bool success, string? redirect = null, string? error = null)
            => Json(new LoginResponse { Success = success, Redirect = redirect, Error = error });

        // todo: support consuming RecoveryCode to bypass password/mfa
        // - on consumption, allow setting new password/mfa secret

        (var success, var error) = await TryLoginUser(request.Username, request.Password, request.TOTP);
        if (!success)
        {
            await notificationService.TryNotifyEvent(NotificationTrigger.AdminLoginFailed, request);
            return createResult(false, error: error);
        }

        await notificationService.TryNotifyEvent(NotificationTrigger.AdminLoginSuccess, request);

        var returnPath = request.ReturnPath;
        if (returnPath?.StartsWith("/") != true) returnPath = $"/";
        return createResult(true, returnPath);
    }

    [HttpPost("/auth/create")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (!ModelState.IsValid) return BadRequest();
        else if (userManager.Users.Any()) return createResult(false, error: "An admin account already exists.");
        else if (serverConfig.CurrentValue.Security.RestrictAdminToLocalhost &&
                 !TKRequestUtils.IsLocalRequest(Request.HttpContext)) return createResult(false);

        var enableTotp = !string.IsNullOrWhiteSpace(request.TOTPSecret);
        if (enableTotp && !TotpUtils.ValidateCode(request.TOTPSecret, request.TOTPCode))
            return createResult(false, error: "Invalid TOTP code");

        IActionResult createResult(bool success, string? redirect = null, string? error = null)
            => Json(new CreateAccountResponse { Success = success, Redirect = redirect, Error = error });

#if DEBUG // delay a bit to test frontend
        await Task.Delay(500);
#endif

        var rawIp = TKRequestUtils.GetIPAddress(Request.HttpContext);
        var ipData = TKIPAddressUtils.ParseIP(rawIp, acceptLocalhostString: true);
        var user = new ApplicationUser
        {
            UserName = request.Username,
            TOTPSecretKey = enableTotp ? request.TOTPSecret : null,
            LastConnectedFromIP = ipData?.IP
        };
        var createUserResult = await userManager.CreateAsync(user, request.Password);
        if (createUserResult.Succeeded == false)
            return createResult(false, error: string.Join(" ", createUserResult.Errors.Select(x => x.Description)));

        // Generate some recovery codes
        await dbContext.RecoveryCodes.AddRangeAsync(Enumerable.Range(0, 8).Select(x => createNewRecoveryCode(user)));
        dbContext.SaveChanges();

        // Login user
        user = await userManager.FindByNameAsync(user.UserName);
        await signInManager.SignInAsync(user!, isPersistent: true);

        return createResult(true, "/");

        static ApplicationUserRecoveryCode createNewRecoveryCode(ApplicationUser user)
        {
            return new ApplicationUserRecoveryCode
            {
                UserId = user.Id, RecoveryCode = $"{Guid.NewGuid()}-{Guid.NewGuid()}-{Guid.NewGuid()}"
            };
        }
    }

    [HttpGet("/auth/logout")]
    public async Task<IActionResult> Logout()
    {
        var user = await userManager.GetUserAsync(User);
        if (user != null && serverConfig.CurrentValue.Security.InvalidateAllSessionsOnAdminLogout)
        {
            await userManager.UpdateSecurityStampAsync(user);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await signInManager.SignOutAsync();

        dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext, $"Logged out manually."));
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Login), new { @return = "/" });
    }

    [HttpGet("/auth/denied")]
    public IActionResult Denied([FromQuery] string? @return = null)
        => RedirectToAction(nameof(Login), new { @return = @return ?? "/", e = "denied" });

    #region Helpers

    private async Task<(bool success, string? error)> TryLoginUser(string username, string password, string? totpCode)
    {
        const string loginErrorMessage = "Invalid username, password or TOTP code.";

        var user = await userManager.FindByNameAsync(username);
        if (user == null) return (success: false, error: loginErrorMessage);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await signInManager.SignOutAsync();

        var usernamePasswordOk = await userManager.CheckPasswordAsync(user, password);
        if (usernamePasswordOk && user.TOTPEnabled)
        {
            if (!TotpUtils.ValidateCode(user.TOTPSecretKey, totpCode))
                return (success: false, error: loginErrorMessage);

            // Update security timestamp before logging in to invalidate any other sessions
            if (serverConfig.CurrentValue.Security.InvalidateAllSessionsOnAdminLogin)
            {
                await userManager.UpdateSecurityStampAsync(user);
            }
        }

        var result = await signInManager.PasswordSignInAsync(user, password, true, false);
        if (!result.Succeeded) return (success: false, error: loginErrorMessage);

        if ((await dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id)) is not ApplicationUser appUser)
            return (success: false, error: "User not found (ERR:2)");

        var rawIp = TKRequestUtils.GetIPAddress(Request.HttpContext);
        var ipData = TKIPAddressUtils.ParseIP(rawIp, acceptLocalhostString: true);
        appUser.LastConnectedFromIP = ipData.IP;

        dbContext.AdminAuditLogEntries.Add(
            new AdminAuditLogEntry(Request.HttpContext, $"Logged in from '{ipData?.IP}'.")
                .TrySetAdminUserId(appUser.Id));

        await dbContext.SaveChangesAsync();

        return (success: true, error: null);
    }

    #endregion

    #region Request models

    [GenerateFrontendModel]
    public class LoginRequest : IProvidesPlaceholders
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? TOTP { get; set; }
        public string? RecoveryCode { get; set; }
        public string? ReturnPath { get; set; }

        public void ProvidePlaceholders(Dictionary<string, string?> values)
        {
            values["Username"] = Username;
        }
    }

    [GenerateFrontendModel]
    public class LoginResponse
    {
        public bool Success { get; internal set; }
        public string? Redirect { get; internal set; }
        public string? Error { get; internal set; }
    }

    [GenerateFrontendModel]
    public class CreateAccountRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? TOTPSecret { get; set; }
        public string? TOTPCode { get; set; }
    }

    [GenerateFrontendModel]
    public class CreateAccountResponse
    {
        public bool Success { get; internal set; }
        public string? Redirect { get; internal set; }
        public string? Error { get; internal set; }
    }

    #endregion
}