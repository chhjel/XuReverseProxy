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

    public bool RewriteDownstreamOrigin { get; set; }
    public bool StripUpstreamSourceTraces { get; set; }

    public bool ShowConditionsNotMet { get; set; }

    public ICollection<ProxyAuthenticationData> Authentications { get; } = [];
    public ICollection<ConditionData> ProxyConditions { get; } = [];
    public ICollection<HtmlTemplate> HtmlTemplateOverrides { get; } = [];

    public void ProvidePlaceholders(Dictionary<string, string?> values)
    {
        values["ProxyConfig.Id"] = Id.ToString();
        values["ProxyConfig.Name"] = Name;
        values["ProxyConfig.Subdomain"] = Subdomain;
        values["ProxyConfig.Port"] = Port?.ToString();
        values["ProxyConfig.ChallengeTitle"] = ChallengeTitle;
        values["ProxyConfig.ChallengeDescription"] = ChallengeDescription;
        values["ProxyConfig.DestinationPrefix"] = DestinationPrefix;
    }
}

[GenerateFrontendModel]
public enum ProxyConfigMode
{
    Forward = 0,
    StaticHTML
}