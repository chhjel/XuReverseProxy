using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class BlockedIpData : IHasId
{
    public Guid Id { get; set; }
    public Guid? RelatedClientId { get; set; }
    public string? Name { get; set; }
    public BlockedIpDataType Type { get; set; }
    public bool Enabled { get; set; }
    public string? IP { get; set; }
    public string? IPRegex { get; set; }
    public string? CidrRange { get; set; }
    public string? Note { get; set; }
    public DateTime BlockedAt { get; set; }
    public DateTime? BlockedUntilUtc { get; set; }
}

[GenerateFrontendModel]
public enum BlockedIpDataType
{
    None,
    IP,
    IPRegex,
    CIDRRange
}
