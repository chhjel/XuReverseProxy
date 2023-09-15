using Microsoft.AspNetCore.Http;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.ProxyAuthentication.Challenges;

[GenerateFrontendModel]
public class ProxyChallengeTypeSecretQueryString : ProxyChallengeTypeBase
{
    public override string Name { get; } = "Secret query string";
    public string? Secret { get; set; }

    public override Task<object> CreateFrontendChallengeModelAsync(ProxyChallengeInvokeContext context)
    {
        return Task.FromResult<object>(new { });
    }

    public override bool AutoCheckSolvedOnLoad(ProxyChallengeInvokeContext context) => HasSecret(context?.HttpContext);

    public bool HasSecret(HttpContext? context)
        => context?.Request?.Query?.Any(x => x.Key == "secret" && PlaceholderUtils.ResolveCommonPlaceholders(x.Value) == Secret) == true;
}
