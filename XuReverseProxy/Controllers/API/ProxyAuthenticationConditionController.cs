using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;
using XuReverseProxy.Core.Extensions;

namespace XuReverseProxy.Controllers.API;

public class ProxyAuthenticationConditionController : EFCrudControllerBase<ProxyAuthenticationCondition>
{
    public ProxyAuthenticationConditionController(ApplicationDbContext context)
        : base(context,
            () => context.ProxyAuthenticationConditions)
    {
    }

    protected override void OnDataModified()
    {
        base.OnDataModified();
        _dbContext.InvalidateCacheFor<ProxyConfig>();
        _dbContext.InvalidateCacheFor<ProxyAuthenticationData>();
    }

    protected override Task<GenericResultData<ProxyAuthenticationCondition>> ValidateEntityAsync(ProxyAuthenticationCondition entity)
    {
        entity.DateTimeUtc1 = entity.DateTimeUtc1.SetKindUtc();
        entity.DateTimeUtc2 = entity.DateTimeUtc2.SetKindUtc();
        return base.ValidateEntityAsync(entity);
    }

    [HttpGet("fromAuthData/{authDataId}")]
    public async Task<GenericResultData<List<ProxyAuthenticationCondition>>> GetFromAuthAsync([FromRoute] Guid authDataId)
    {
        try
        {
            var items = await _entities()
                .Where(x => x.AuthenticationDataId == authDataId)
                .ToListAsync();
            return GenericResult.CreateSuccess(items);
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError<List<ProxyAuthenticationCondition>>(ex.Message);
        }
    }
    public override async Task<GenericResultData<ProxyAuthenticationCondition>> CreateOrUpdateEntityAsync([FromBody] ProxyAuthenticationCondition entity)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<ProxyAuthenticationCondition>(ModelState);

        var isNew = entity.Id == Guid.Empty;
        var result = await base.CreateOrUpdateEntityAsync(entity);
        if (result.Success)
        {
            var auth = await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == entity.AuthenticationDataId);
            var config = auth == null ? null : await _dbContext.ProxyConfigs.FirstOrDefaultAsync(x => x.Id == auth.ProxyConfigId);
            _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext,
                    isNew
                    ? $"Created new {entity.ConditionType}-condition for {auth?.ChallengeTypeId}-auth / {AdminAuditLogEntry.Placeholder_ProxyConfig}"
                    : $"Updated {entity.ConditionType}-condition for {auth?.ChallengeTypeId}-auth / {AdminAuditLogEntry.Placeholder_ProxyConfig}")
                    .SetRelatedProxyConfig(config?.Id, config?.Name)
                );
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }

    public override async Task<GenericResult> DeleteEntityAsync([FromRoute] Guid entityId)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<ProxyAuthenticationCondition>(ModelState);

        var entity = (await GetEntityAsync(entityId))?.Data;
        var result = await base.DeleteEntityAsync(entityId);
        if (result.Success)
        {
            var auth = entity == null ? null : await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == entity.AuthenticationDataId);
            var config = auth == null ? null : await _dbContext.ProxyConfigs.FirstOrDefaultAsync(x => x.Id == auth.ProxyConfigId);
            _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext, $"Deleted {entity?.ConditionType}-condition for {auth?.ChallengeTypeId}-auth / {AdminAuditLogEntry.Placeholder_ProxyConfig}")
                    .SetRelatedProxyConfig(config?.Id, config?.Name)
                );
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }
}
