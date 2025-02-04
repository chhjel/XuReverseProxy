using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class GlobalVariable : IHasId
{
    public Guid Id { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
    public string? LastUpdatedBy { get; set; }
    public string? LastUpdatedSourceIP { get; set; }

    public string? Name { get; set; }
    public string? Value { get; set; }
}