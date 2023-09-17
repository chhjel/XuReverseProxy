using Microsoft.AspNetCore.Identity;
using XuReverseProxy.Core.Abstractions;

namespace XuReverseProxy.Core.Models.DbEntity;

public class ApplicationUser : IdentityUser, IProvidesPlaceholders
{
    public string? LastConnectedFromIP { get; set; }
    public string? TOTPSecretKey { get; set; }
    public ICollection<ApplicationUserRecoveryCode> RecoveryCodes { get; } = new List<ApplicationUserRecoveryCode>();

    public bool TOTPEnabled => !string.IsNullOrWhiteSpace(TOTPSecretKey);

    public string ResolvePlaceholders(string template, Func<string?, string?> transformer)
    {
        return template
            .Replace("{{User.Username}}", transformer(UserName), StringComparison.OrdinalIgnoreCase)
            .Replace("{{User.IP}}", transformer(LastConnectedFromIP), StringComparison.OrdinalIgnoreCase);
    }
}
