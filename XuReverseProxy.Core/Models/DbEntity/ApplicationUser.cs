using Microsoft.AspNetCore.Identity;
using XuReverseProxy.Core.Abstractions;

namespace XuReverseProxy.Core.Models.DbEntity;

public class ApplicationUser : IdentityUser, IProvidesPlaceholders
{
    public string? LastConnectedFromIP { get; set; }
    public string? TOTPSecretKey { get; set; }
    public ICollection<ApplicationUserRecoveryCode> RecoveryCodes { get; } = [];

    public bool TOTPEnabled => !string.IsNullOrWhiteSpace(TOTPSecretKey);

    public void ProvidePlaceholders(Dictionary<string, string?> values)
    {
        values["User.Id"] = Id;
        values["User.Username"] = UserName;
        values["User.IP"] = LastConnectedFromIP;
    }
}
