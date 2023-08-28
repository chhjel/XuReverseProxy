namespace XuReverseProxy.Core.Models.DbEntity;

public class ProxyAuthenticationData
{
    public Guid Id { get; set; }
    public Guid ProxyConfigId { get; set; }
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
}
