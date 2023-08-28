﻿using Microsoft.AspNetCore.Http;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.ProxyAuthentication.Challenges;

public class ProxyChallengeTypeSecretQueryString : ProxyChallengeTypeBase
{
    public string? Secret { get; set; }

    public override Task<object> CreateFrontendChallengeModelAsync(ProxyChallengeInvokeContext context)
    {
        return Task.FromResult<object>(new { });
    }

    public override bool AutoCheckSolvedOnLoad(ProxyChallengeInvokeContext context) => HasSecret(context?.HttpContext);

    public bool HasSecret(HttpContext? context)
        => context?.Request?.Query?.Any(x => x.Key == "secret" && PlaceholderUtils.ResolveCommonPlaceholders(x.Value) == Secret) == true;
}
