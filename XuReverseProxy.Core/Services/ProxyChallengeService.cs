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

public class ProxyChallengeService(ApplicationDbContext dbContext,
    IConditionChecker conditionChecker, IProxyClientIdentityService proxyClientIdentityService,
    IHttpContextAccessor httpContextAccessor, IProxyAuthenticationChallengeFactory proxyAuthenticationChallengeFactory, INotificationService notificationService) : IProxyChallengeService
{
    public async Task<bool> IsChallengeSolvedAsync(Guid identityId, ProxyAuthenticationData auth)
        => await IsChallengeSolvedAsync(identityId, auth.Id, auth.SolvedId, auth.SolvedDuration);

    public async Task<bool> IsChallengeSolvedAsync(Guid identityId, Guid authenticationId, Guid solvedId, TimeSpan? solvedDuration)
        => await dbContext.ProxyClientIdentitySolvedChallengeDatas.AnyAsync(x =>
                x.IdentityId == identityId
                && x.AuthenticationId == authenticationId
                && x.SolvedId == solvedId
                && (solvedDuration == null || (DateTime.UtcNow - x.SolvedAtUtc) < solvedDuration)
        );

    public async Task<ProxyClientIdentitySolvedChallengeData?> GetSolvedChallengeSolvedDataAsync(Guid identityId, Guid authenticationId, Guid solvedId, TimeSpan? solvedDuration)
        => await dbContext.ProxyClientIdentitySolvedChallengeDatas.FirstOrDefaultAsync(x =>
                x.IdentityId == identityId
                && x.AuthenticationId == authenticationId
                && x.SolvedId == solvedId
                && (solvedDuration == null || (DateTime.UtcNow - x.SolvedAtUtc) < solvedDuration)
        );

    public async Task<bool> ChallengeRequirementPassedAsync(Guid authenticationDataId, ConditionContext conditionContext)
    {
        var conditions = (await dbContext.GetWithCacheAsync(x => x.ConditionDatas)).Where(x => x.ParentId == authenticationDataId);
        return conditionChecker.ConditionsPassed(conditions, conditionContext);
    }

    public async Task<(ConditionData.ConditionType Type, int Group, string Summary, bool Passed)[]> GetChallengeRequirementDataAsync(Guid authenticationDataId, ConditionContext conditionContext)
    {
        var conditions = (await dbContext.GetWithCacheAsync(x => x.ConditionDatas)).Where(x => x.ParentId == authenticationDataId);
        return conditions.AsEnumerable()
            .Select(c => (
                c.Type,
                c.Group,
                Summary: c.CreateSummary(),
                Passed: conditionChecker.ConditionPassed(c, conditionContext)
            ))
            .ToArray();
    }

    public async Task<bool> SetChallengeSolvedAsync(Guid identityId, Guid authenticationId, Guid solvedId)
    {
        var identity = await proxyClientIdentityService.GetProxyClientIdentityAsync(identityId);
        if (identity == null) return false;

        var auth = await dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return false;

        var proxyConfig = await dbContext.ProxyConfigs.FirstOrDefaultAsync(x => x.Id == auth.ProxyConfigId);
        if (proxyConfig == null) return false;

        var data = await dbContext.ProxyClientIdentitySolvedChallengeDatas.FirstOrDefaultAsync(x =>
            x.IdentityId == identityId
            && x.AuthenticationId == authenticationId
        );
        if (data != null)
        {
            data.SolvedId = solvedId;
            data.SolvedAtUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
        }
        else
        {
            await dbContext.ProxyClientIdentitySolvedChallengeDatas.AddAsync(new ProxyClientIdentitySolvedChallengeData
            {
                IdentityId = identityId,
                AuthenticationId = authenticationId,
                SolvedId = solvedId,
                SolvedAtUtc = DateTime.UtcNow
            });

            var authBaseData = proxyAuthenticationChallengeFactory.GetBaseChallengeData(auth.ChallengeTypeId);
            var challengeLogName = authBaseData?.Name ?? auth.ChallengeTypeId;
            dbContext.ClientAuditLogEntries.Add(new ClientAuditLogEntry(httpContextAccessor.HttpContext, identityId, identity.NameForLog(), 
                $"Completed challenge '{challengeLogName}' for {ClientAuditLogEntry.Placeholder_ProxyConfig}")
                .SetRelatedProxyConfig(auth.ProxyConfigId, proxyConfig.Name));

            await dbContext.SaveChangesAsync();
        }

        await notificationService.TryNotifyEvent(NotificationTrigger.ClientCompletedChallenge, identity, auth, proxyConfig);
        return true;
    }

    public async Task<bool> SetChallengeUnsolvedAsync(Guid identityId, Guid authenticationId, Guid solvedId)
    {
        var identity = await proxyClientIdentityService.GetProxyClientIdentityAsync(identityId);
        if (identity == null) return false;

        var auth = await dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return false;

        var proxyConfig = await dbContext.ProxyConfigs.FirstOrDefaultAsync(x => x.Id == auth.ProxyConfigId);
        if (proxyConfig == null) return false;

        var data = await dbContext.ProxyClientIdentitySolvedChallengeDatas.FirstOrDefaultAsync(x =>
            x.IdentityId == identityId
            && x.AuthenticationId == authenticationId
        );
        if (data == null) return false;

        dbContext.Remove(data);

        var authBaseData = proxyAuthenticationChallengeFactory.GetBaseChallengeData(auth.ChallengeTypeId);
        var challengeLogName = authBaseData?.Name ?? auth.ChallengeTypeId;
        dbContext.ClientAuditLogEntries.Add(new ClientAuditLogEntry(httpContextAccessor.HttpContext, identityId, identity.NameForLog(),
            $"Un-completed challenge '{challengeLogName}' for {ClientAuditLogEntry.Placeholder_ProxyConfig}")
            .SetRelatedProxyConfig(auth.ProxyConfigId, proxyConfig.Name));

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<int> ResetChallengesForAuthenticationAsync(Guid authenticationId, Guid? identityId = null)
    {
        var auth = await dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId);
        if (auth == null) return 0;

        var query = dbContext.ProxyClientIdentitySolvedChallengeDatas
            .Where(x => x.AuthenticationId == authenticationId);

        if (identityId != null) query = query.Where(x => x.IdentityId == identityId);

        var affectedRows = await query.ExecuteDeleteAsync();
        return affectedRows;
    }
}
