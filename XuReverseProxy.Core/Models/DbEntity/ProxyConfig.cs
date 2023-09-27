using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class ProxyConfig : IHasId, IProvidesPlaceholders
{
    public Guid Id { get; set; }
    public bool Enabled { get; set; }
    public string? Name { get; set; }
    public string? Subdomain { get; set; }
    public int? Port { get; set; }

    public string? ChallengeTitle { get; set; }
    public string? ChallengeDescription { get; set; }

    public bool ShowCompletedChallenges { get; set; }
    public bool ShowChallengesWithUnmetRequirements { get; set; }

    public ProxyConfigMode Mode { get; set; }
    public string? DestinationPrefix { get; set; }
    public string? StaticHTML { get; set; }

    public ICollection<ProxyAuthenticationData> Authentications { get; } = new List<ProxyAuthenticationData>();
    public ICollection<ProxyCondition> ProxyConditions { get; } = new List<ProxyCondition>();

    public string ResolvePlaceholders(string template, Func<string?, string?> transformer)
    {
        return template
            .Replace("{{ProxyConfig.Id}}", transformer(Id.ToString()), StringComparison.OrdinalIgnoreCase)
            .Replace("{{ProxyConfig.Name}}", transformer(Name), StringComparison.OrdinalIgnoreCase)
            .Replace("{{ProxyConfig.Subdomain}}", transformer(Subdomain), StringComparison.OrdinalIgnoreCase)
            .Replace("{{ProxyConfig.Port}}", transformer(Port?.ToString()), StringComparison.OrdinalIgnoreCase)
            .Replace("{{ProxyConfig.ChallengeTitle}}", transformer(ChallengeTitle), StringComparison.OrdinalIgnoreCase)
            .Replace("{{ProxyConfig.ChallengeDescription}}", transformer(ChallengeDescription), StringComparison.OrdinalIgnoreCase)
            .Replace("{{ProxyConfig.DestinationPrefix}}", transformer(DestinationPrefix), StringComparison.OrdinalIgnoreCase);
    }
}

[GenerateFrontendModel]
public enum ProxyConfigMode
{
    Forward = 0,
    StaticHTML
}
