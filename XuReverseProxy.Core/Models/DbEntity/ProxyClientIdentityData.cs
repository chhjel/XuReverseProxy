using System.Text.Json.Serialization;
using XuReverseProxy.Core.Abstractions;

namespace XuReverseProxy.Core.Models.DbEntity;

public class ProxyClientIdentityData : IHasId
{
    public Guid Id { get; set; }
    public Guid IdentityId { get; set; }
    public Guid? AuthenticationId { get; set; }
    [JsonIgnore]
    public ProxyClientIdentity Identity { get; set; } = null!;
    public required string Key { get; set; }
    public string? Value { get; set; }
}
