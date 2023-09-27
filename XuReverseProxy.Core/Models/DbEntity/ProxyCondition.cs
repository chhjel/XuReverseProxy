using System.Text.Json.Serialization;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class ProxyCondition : IHasId
{
    public Guid Id { get; set; }
    public Guid ProxyConfigId { get; set; }
    [JsonIgnore]
    public ProxyConfig ProxyConfig { get; set; } = null!;
    //public ProxyConditionType ConditionType { get; set; }
}
