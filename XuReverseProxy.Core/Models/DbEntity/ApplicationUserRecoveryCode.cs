namespace XuReverseProxy.Core.Models.DbEntity;

public class ApplicationUserRecoveryCode
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public required string RecoveryCode { get; set; }
}
