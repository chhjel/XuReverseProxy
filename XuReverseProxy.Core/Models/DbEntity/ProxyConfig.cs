namespace XuReverseProxy.Core.Models.DbEntity;

public class ProxyConfig
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

    public string? DestinationPrefix { get; set; }
    public ICollection<ProxyAuthenticationData> Authentications { get; } = new List<ProxyAuthenticationData>();
}
