using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Extensions;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.ProxyAuthentication;

namespace XuReverseProxy.Core.Services;

public interface IProxyChallengeService
{
    Task<bool> ChallengeRequirementPassedAsync(Guid authenticationDataId, ConditionContext conditionContext);
    Task<(ConditionData.ConditionType Type, int Group, string Summary, bool Passed)[]> GetChallengeRequirementDataAsync(Guid authenticationDataId, ConditionContext conditionContext);
    Task<bool> SetChallengeSolvedAsync(Guid identityId, Guid authenticationId, Guid solvedId);
    Task<bool> SetChallengeUnsolvedAsync(Guid identityId, Guid authenticationId, Guid solvedId);
    Task<bool> IsChallengeSolvedAsync(Guid identityId, ProxyAuthenticationData auth);
    Task<bool> IsChallengeSolvedAsync(Guid identityId, Guid authenticationId, Guid solvedId, TimeSpan? solvedDuration);
    Task<ProxyClientIdentitySolvedChallengeData?> GetSolvedChallengeSolvedDataAsync(Guid identityId, Guid authenticationId, Guid solvedId, TimeSpan? solvedDuration);
    Task<int> ResetChallengesForAuthenticationAsync(Guid authenticationId, Guid? identityId = null);
}

public class ProxyChallengeService : IProxyChallengeService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConditionChecker _conditionChecker;
    private readonly IProxyClientIdentityService _proxyClientIdentityService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProxyAuthenticationChallengeFactory _proxyAuthenticationChallengeFactory;
    private readonly INotificationService _notificationService;

    public ProxyChallengeService(ApplicationDbContext dbContext,
        IConditionChecker conditionChecker, IProxyClientIdentityService proxyClientIdentityService,
        IHttpContextAccessor httpContextAccessor, IProxyAuthenticationChallengeFactory proxyAuthenticationChallengeFactory, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _conditionChecker = conditionChecker;
        _proxyClientIdentityService = proxyClientIdentityService;
        _httpContextAccessor = httpContextAccessor;
        _proxyAuthenticationChallengeFactory = proxyAuthenticationChallengeFactory;
        _notificationService = notificationService;
    }

    public async Task<bool> IsChallengeSolvedAsync(Guid identityId, ProxyAuthenticationData auth)
        => await IsChallengeSolvedAsync(identityId, auth.Id, auth.SolvedId, auth.SolvedDuration);

    public async Task<bool> IsChallengeSolvedAsync(Guid identityId, Guid authenticationId, Guid solvedId, TimeSpan? solvedDuration)
        => await _dbContext.ProxyClientIdentitySolvedChallengeDatas.AnyAsync(x =>
                x.IdentityId == identityId
                && x.AuthenticationId == authenticationId
                && x.SolvedId == solvedId
                && (solvedDuration == null || (DateTime.UtcNow - x.SolvedAtUtc) < solvedDuration)
        );

    public async Task<ProxyClientIdentitySolvedChallengeData?> GetSolvedChallengeSolvedDataAsync(Guid identityId, Guid authenticationId, Guid solvedId, TimeSpan? solvedDuration)
        => await _dbContext.ProxyClientIdentitySolvedChallengeDatas.FirstOrDefaultAsync(x =>
                x.IdentityId == identityId
                && x.AuthenticationId == authenticationId
                && x.SolvedId == solvedId
                && (solvedDuration == null || (DateTime.UtcNow - x.SolvedAtUtc) < solvedDuration)
        );

    public async Task<bool> ChallengeRequirementPassedAsync(Guid authenticationDataId, ConditionContext conditionContext)
    {
        var conditions = (await _dbContext.GetWithCacheAsync(x => x.ConditionDatas)).Where(x => x.ParentId == authenticationDataId);
        return _conditionChecker.ConditionsPassed(conditions, conditionContext);
    }

    public async Task<(ConditionData.ConditionType Type, int Group, string Summary, bool Passed)[]> GetChallengeRequirementDataAsync(Guid authenticationDataId, ConditionContext conditionContext)
    {
        var conditions = (await _dbContext.GetWithCacheAsync(x => x.ConditionDatas)).Where(x => x.ParentId == authenticationDataId);
        return conditions.AsEnumerable()
            .Select(c => (
                c.Type,
                c.Group,
                Summary: c.CreateSummary(),
                Passed: _conditionChecker.ConditionPassed(c, conditionContext)
            ))
            .ToArray();
    }

    public async Task<bool> SetChallengeSolvedAsync(Guid identityId, Guid authenticationId, Guid solvedId)
    {
        var identity = await _proxyClientIdentityService.GetProxyClientIdentityAsync(identityId);
        if (identity == null) return false;

        var auth = await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return false;

        var proxyConfig = await _dbContext.ProxyConfigs.FirstOrDefaultAsync(x => x.Id == auth.ProxyConfigId);
        if (proxyConfig == null) return false;

        var data = await _dbContext.ProxyClientIdentitySolvedChallengeDatas.FirstOrDefaultAsync(x =>
            x.IdentityId == identityId
            && x.AuthenticationId == authenticationId
        );
        if (data != null)
        {
            data.SolvedId = solvedId;
            data.SolvedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            await _dbContext.ProxyClientIdentitySolvedChallengeDatas.AddAsync(new ProxyClientIdentitySolvedChallengeData
            {
                IdentityId = identityId,
                AuthenticationId = authenticationId,
                SolvedId = solvedId,
                SolvedAtUtc = DateTime.UtcNow
            });

            var authBaseData = _proxyAuthenticationChallengeFactory.GetBaseChallengeData(auth.ChallengeTypeId);
            var challengeLogName = authBaseData?.Name ?? auth.ChallengeTypeId;
            _dbContext.ClientAuditLogEntries.Add(new ClientAuditLogEntry(_httpContextAccessor.HttpContext, identityId, identity.NameForLog(), 
                $"Completed challenge '{challengeLogName}' for {ClientAuditLogEntry.Placeholder_ProxyConfig}")
                .SetRelatedProxyConfig(auth.ProxyConfigId, proxyConfig.Name));

            await _dbContext.SaveChangesAsync();
        }

        await _notificationService.TryNotifyEvent(NotificationTrigger.ClientCompletedChallenge, identity, auth, proxyConfig);
        return true;
    }

    public async Task<bool> SetChallengeUnsolvedAsync(Guid identityId, Guid authenticationId, Guid solvedId)
    {
        var identity = await _proxyClientIdentityService.GetProxyClientIdentityAsync(identityId);
        if (identity == null) return false;

        var auth = await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return false;

        var proxyConfig = await _dbContext.ProxyConfigs.FirstOrDefaultAsync(x => x.Id == auth.ProxyConfigId);
        if (proxyConfig == null) return false;

        var data = await _dbContext.ProxyClientIdentitySolvedChallengeDatas.FirstOrDefaultAsync(x =>
            x.IdentityId == identityId
            && x.AuthenticationId == authenticationId
        );
        if (data == null) return false;

        _dbContext.Remove(data);

        var authBaseData = _proxyAuthenticationChallengeFactory.GetBaseChallengeData(auth.ChallengeTypeId);
        var challengeLogName = authBaseData?.Name ?? auth.ChallengeTypeId;
        _dbContext.ClientAuditLogEntries.Add(new ClientAuditLogEntry(_httpContextAccessor.HttpContext, identityId, identity.NameForLog(),
            $"Un-completed challenge '{challengeLogName}' for {ClientAuditLogEntry.Placeholder_ProxyConfig}")
            .SetRelatedProxyConfig(auth.ProxyConfigId, proxyConfig.Name));

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<int> ResetChallengesForAuthenticationAsync(Guid authenticationId, Guid? identityId = null)
    {
        var auth = await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId);
        if (auth == null) return 0;

        var query = _dbContext.ProxyClientIdentitySolvedChallengeDatas
            .Where(x => x.AuthenticationId == authenticationId);

        if (identityId != null) query = query.Where(x => x.IdentityId == identityId);

        var affectedRows = await query.ExecuteDeleteAsync();
        return affectedRows;
    }
}
