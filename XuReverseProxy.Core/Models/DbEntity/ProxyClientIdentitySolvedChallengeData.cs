namespace XuReverseProxy.Core.Models.DbEntity;

public class ProxyClientIdentitySolvedChallengeData
{
    public Guid Id { get; set; }
    public Guid AuthenticationId { get; set; }
    public Guid IdentityId { get; set; }
    public ProxyClientIdentity Identity { get; set; } = null!;
    public Guid SolvedId { get; set; }
    public DateTime SolvedAtUtc { get; set; }
}
