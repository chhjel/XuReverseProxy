using System.Text.Json.Serialization;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
[Serializable]
public class ProxyAuthenticationData : IHasId, IProvidesPlaceholders
{
    public Guid Id { get; set; }
    public Guid ProxyConfigId { get; set; }
    [JsonIgnore]
    public ProxyConfig ProxyConfig { get; set; } = null!;
    public int Order { get; set; }

    /// <summary>
    /// Id of the challenge type used.
    /// </summary>
    public string? ChallengeTypeId { get; set; }

    /// <summary>Serialized data of the challenge.</summary>
    public string? ChallengeJson { get; set; }

    /// <summary>
    /// Id to be added to list of solved auth challenges. Separate property to allow invalidation.
    /// </summary>
    public Guid SolvedId { get; set; }

    public TimeSpan? SolvedDuration { get; set; }
    public ICollection<ProxyAuthenticationCondition> Conditions { get; } = new List<ProxyAuthenticationCondition>();

    public string ResolvePlaceholders(string template, Func<string?, string?> transformer)
    {
        return template
            .Replace("{{Auth.Id}}", transformer(Id.ToString()), StringComparison.OrdinalIgnoreCase)
            .Replace("{{Auth.ChallengeTypeId}}", transformer(ChallengeTypeId), StringComparison.OrdinalIgnoreCase);
    }
}
