using System.Text.Json;
using XuReverseProxy.Core.ProxyAuthentication.Challenges;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.ProxyAuthentication;

public interface IProxyAuthenticationChallengeFactory
{
    ProxyChallengeTypeBase? GetBaseChallengeData(string? authTypeId);
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

    public ProxyChallengeTypeBase? GetBaseChallengeData(string? authTypeId)
    {
        if (string.IsNullOrWhiteSpace(authTypeId)) return null;

        lock (_baseChallengeDataCache)
        {
            if (_baseChallengeDataCache.Count == 0)
            {
                foreach (var kvp in _challengeTypes)
                {
                    var authType = ResolveAuthenticationType(authTypeId);
                    if (authType == null) continue;

                    if (Activator.CreateInstance(authType) is ProxyChallengeTypeBase instance)
                    {
                        _baseChallengeDataCache[kvp.Key] = instance;
                    }
                }
            }

            return _baseChallengeDataCache.TryGetValue(authTypeId, out var data) ? data : null;
        }
    }

    private static readonly Dictionary<string, ProxyChallengeTypeBase> _baseChallengeDataCache = new();

    private static readonly Dictionary<string, Type> _challengeTypes = new()
    {
        { nameof(ProxyChallengeTypeLogin), typeof(ProxyChallengeTypeLogin) },
        { nameof(ProxyChallengeTypeAdminLogin), typeof(ProxyChallengeTypeAdminLogin) },
        { nameof(ProxyChallengeTypeOTP), typeof(ProxyChallengeTypeOTP) },
        { nameof(ProxyChallengeTypeManualApproval), typeof(ProxyChallengeTypeManualApproval) },
        { nameof(ProxyChallengeTypeSecretQueryString), typeof(ProxyChallengeTypeSecretQueryString) }
    };

    private static Type? ResolveAuthenticationType(string authTypeId)
        => _challengeTypes.TryGetValue(authTypeId, out var type) ? type : null;
}
