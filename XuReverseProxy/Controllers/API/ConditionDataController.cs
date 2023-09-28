using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Extensions;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class ConditionDataController : EFCrudControllerBase<ConditionData>
{
    public ConditionDataController(ApplicationDbContext context)
        : base(context,
            () => context.Conditions)
    {
    }

    protected override void OnDataModified()
    {
        base.OnDataModified();
        _dbContext.InvalidateCacheFor<ProxyConfig>();
        _dbContext.InvalidateCacheFor<ProxyAuthenticationData>();
    }

    protected override Task<GenericResultData<ConditionData>> ValidateEntityAsync(ConditionData entity)
    {
        entity.DateTimeUtc1 = entity.DateTimeUtc1.SetKindUtc();
        entity.DateTimeUtc2 = entity.DateTimeUtc2.SetKindUtc();
        return base.ValidateEntityAsync(entity);
    }

    public override async Task<GenericResultData<ConditionData>> CreateOrUpdateEntityAsync([FromBody] ConditionData entity)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<ConditionData>(ModelState);

        var isNew = entity.Id == Guid.Empty;
        if (isNew)
        {
            // Update relations
            string? hint = null;
            if (Request.Headers.TryGetValue(HeaderName_Hint, out var hintValues)) hint = hintValues.FirstOrDefault();
            else if (string.IsNullOrWhiteSpace(hint) || hint == "none") throw new ArgumentException("Invalid hint value.", nameof(entity));

            if (hint == nameof(ProxyConfig))
            {
                var parent = await _dbContext.ProxyConfigs.FirstOrDefaultAsync(x => x.Id == entity.ParentId);
                parent!.ProxyConditions.Add(entity);
            }
            else if (hint == nameof(ProxyAuthenticationData))
            {
                var parent = await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == entity.ParentId);
                parent!.Conditions.Add(entity);
            }
        }

        var result = await base.CreateOrUpdateEntityAsync(entity);
        if (result.Success)
        {
            var auth = await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == entity.ParentId);
            var config = auth == null ? null : await _dbContext.ProxyConfigs.FirstOrDefaultAsync(x => x.Id == auth.ProxyConfigId);
            _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext,
                    isNew
                    ? $"Created new {entity.Type}-condition for {auth?.ChallengeTypeId}-auth / {AdminAuditLogEntry.Placeholder_ProxyConfig}"
                    : $"Updated {entity.Type}-condition for {auth?.ChallengeTypeId}-auth / {AdminAuditLogEntry.Placeholder_ProxyConfig}")
                    .SetRelatedProxyConfig(config?.Id, config?.Name)
                );
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }

    public override async Task<GenericResult> DeleteEntityAsync([FromRoute] Guid entityId)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<ConditionData>(ModelState);

        var entity = (await GetEntityAsync(entityId))?.Data;
        var result = await base.DeleteEntityAsync(entityId);
        if (result.Success)
        {
            var auth = entity == null ? null : await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == entity.ParentId);
            var config = auth == null ? null : await _dbContext.ProxyConfigs.FirstOrDefaultAsync(x => x.Id == auth.ProxyConfigId);
            _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext, $"Deleted {entity?.Type}-condition for {auth?.ChallengeTypeId}-auth / {AdminAuditLogEntry.Placeholder_ProxyConfig}")
                    .SetRelatedProxyConfig(config?.Id, config?.Name)
                );
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }
}
