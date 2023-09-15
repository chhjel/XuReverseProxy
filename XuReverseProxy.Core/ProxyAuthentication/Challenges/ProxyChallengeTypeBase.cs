namespace XuReverseProxy.Core.ProxyAuthentication.Challenges;

public abstract class ProxyChallengeTypeBase
{
    public virtual string Name => GetType().Name;
    public virtual string TypeId => GetType().Name;
    public abstract Task<object> CreateFrontendChallengeModelAsync(ProxyChallengeInvokeContext context);
    public virtual bool AutoCheckSolvedOnLoad(ProxyChallengeInvokeContext context) => false;
}
