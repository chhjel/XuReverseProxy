using Microsoft.AspNetCore.Identity;

namespace XuReverseProxy.Core.Models.DbEntity;

public class ApplicationUser : IdentityUser
{
    public string? LastConnectedFromIP { get; set; }
    public string? TOTPSecretKey { get; set; }
    public ICollection<ApplicationUserRecoveryCode> RecoveryCodes { get; } = new List<ApplicationUserRecoveryCode>();

    public bool TOTPEnabled => !string.IsNullOrWhiteSpace(TOTPSecretKey);
}
