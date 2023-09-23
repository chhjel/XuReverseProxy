using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Services;

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

    public override async Task<bool> AutoCheckSolvedOnLoadAsync(ProxyChallengeInvokeContext context)
    {
        var query = context.HttpContext?.Request?.Query;
        if (query?.Any() != true) return false;

        var placeholderResolver = context.GetService<IPlaceholderResolver>();
        foreach(var kvp in query)
        {
            if (kvp.Key != "secret") continue;

            var value = await placeholderResolver.ResolvePlaceholdersAsync(kvp.Value);
            if (value?.Equals(Secret, StringComparison.InvariantCulture) == true) return true;
        }

        return false;
    }
}
