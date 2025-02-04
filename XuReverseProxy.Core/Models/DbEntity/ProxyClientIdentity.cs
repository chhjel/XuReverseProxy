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

    public ICollection<ProxyClientIdentitySolvedChallengeData> SolvedChallenges { get; } = [];
    public ICollection<ProxyClientIdentityData> Data { get; } = [];

    public void ProvidePlaceholders(Dictionary<string, string?> values)
    {
        values["Client.Id"] = Id.ToString();
        values["Client.IP"] = IP;
        values["Client.Note"] = Note;
        values["Client.UserAgent"] = UserAgent;
        values["Client.BlockedMessage"] = BlockedMessage;
    }
}