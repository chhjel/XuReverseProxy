using System.Text.Json.Serialization;
using XuReverseProxy.Core.Abstractions;

namespace XuReverseProxy.Core.Models.DbEntity;

public class ProxyClientIdentitySolvedChallengeData : IHasId
{
    public Guid Id { get; set; }
    public Guid AuthenticationId { get; set; }
    public Guid IdentityId { get; set; }
    [JsonIgnore]
    public ProxyClientIdentity Identity { get; set; } = null!;
    public Guid SolvedId { get; set; }
    public DateTime SolvedAtUtc { get; set; }
}
