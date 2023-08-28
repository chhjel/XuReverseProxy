namespace XuReverseProxy.Core.Models.DbEntity;

public class ProxyClientIdentityData
{
    public Guid Id { get; set; }
    public Guid IdentityId { get; set; }
    public Guid? AuthenticationId { get; set; }
    public ProxyClientIdentity Identity { get; set; } = null!;
    public required string Key { get; set; }
    public string? Value { get; set; }
}
