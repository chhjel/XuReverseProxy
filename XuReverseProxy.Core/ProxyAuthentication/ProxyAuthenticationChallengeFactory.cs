using System.Text.Json;
using XuReverseProxy.Core.ProxyAuthentication.Challenges;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.ProxyAuthentication;

public interface IProxyAuthenticationChallengeFactory
{
    ProxyChallengeTypeBase? CreateProxyAuthenticationChallenge(string? authTypeId, string? authTypeJson);
}

public class ProxyAuthenticationChallengeFactory : IProxyAuthenticationChallengeFactory
{
    public ProxyChallengeTypeBase? CreateProxyAuthenticationChallenge(string? authTypeId, string? authTypeJson)
    {
        if (string.IsNullOrWhiteSpace(authTypeId)) return null;

        var authType = ResolveAuthenticationType(authTypeId);
        if (authType == null) return null;
        
        if (string.IsNullOrWhiteSpace(authTypeJson)) return Activator.CreateInstance(authType) as ProxyChallengeTypeBase;
        else return JsonSerializer.Deserialize(authTypeJson, authType, JsonConfig.DefaultOptions) as ProxyChallengeTypeBase;
    }

    private static readonly Dictionary<string, Type> _challengeTypes = new()
    {
        { nameof(ProxyChallengeTypeLogin), typeof(ProxyChallengeTypeLogin) },
        { nameof(ProxyChallengeTypeOTP), typeof(ProxyChallengeTypeOTP) },
        { nameof(ProxyChallengeTypeManualApproval), typeof(ProxyChallengeTypeManualApproval) },
        { nameof(ProxyChallengeTypeSecretQueryString), typeof(ProxyChallengeTypeSecretQueryString) }
    };

    private static Type? ResolveAuthenticationType(string authTypeId)
        => _challengeTypes.TryGetValue(authTypeId, out var type) ? type : null;
}
