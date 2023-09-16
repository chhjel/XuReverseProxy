using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class ProxyClientIdentity : IProvidesPlaceholders, IHasId
{
    public Guid Id { get; set; }

    public string? IP { get; set; }
    public string? UserAgent { get; set; }
    //public string? Fingerprint { get; set; }
    public string? Note { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastAttemptedAccessedAtUtc { get; set; }
    public DateTime? LastAccessedAtUtc { get; set; }
    //public DateTime? LastNotifiedAccessAtUtc { get; set; }

    public bool Blocked { get; set; }
    public DateTime? BlockedAtUtc { get; set; }
    public string? BlockedMessage { get; set; }

    public ICollection<ProxyClientIdentitySolvedChallengeData> SolvedChallenges { get; } = new List<ProxyClientIdentitySolvedChallengeData>();
    public ICollection<ProxyClientIdentityData> Data { get; } = new List<ProxyClientIdentityData>();

    public string ResolvePlaceholders(string template, Func<string?, string?> transformer)
    {
        return template
            .Replace("{{Client.IP}}", transformer(IP), StringComparison.OrdinalIgnoreCase)
            .Replace("{{Client.Note}}", transformer(Note), StringComparison.OrdinalIgnoreCase)
            .Replace("{{Client.UserAgent}}", transformer(UserAgent), StringComparison.OrdinalIgnoreCase)
            .Replace("{{Client.BlockedMessage}}", transformer(BlockedMessage), StringComparison.OrdinalIgnoreCase);
    }
}
