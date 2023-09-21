using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class ProxyConfigController : EFCrudControllerBase<ProxyConfig>
{
    public ProxyConfigController(ApplicationDbContext context)
        : base(context, () => context.ProxyConfigs)
    {
    }

    public override async Task<GenericResultData<ProxyConfig>> CreateOrUpdateEntityAsync([FromBody] ProxyConfig entity)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<ProxyConfig>(ModelState);

        var isNew = entity.Id == Guid.Empty;
        var result = await base.CreateOrUpdateEntityAsync(entity);
        if (result.Success)
        {
            _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext,
                    isNew
                    ? $"Created new proxy config {AdminAuditLogEntry.Placeholder_ProxyConfig}"
                    : $"Updated proxy config {AdminAuditLogEntry.Placeholder_ProxyConfig}")
                    .SetRelatedProxyConfig(result.Data?.Id, result.Data?.Name)
                );
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }

    public override async Task<GenericResult> DeleteEntityAsync([FromRoute] Guid entityId)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<ProxyConfig>(ModelState);

        var entity = await GetEntityAsync(entityId);
        var result = await base.DeleteEntityAsync(entityId);
        if (result.Success)
        {
            _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext, $"Deleted proxy config {AdminAuditLogEntry.Placeholder_ProxyConfig}")
                    .SetRelatedProxyConfig(entityId, entity?.Data?.Name)
                );
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }

    protected override async Task<GenericResultData<ProxyConfig>> ValidateEntityAsync(ProxyConfig entity)
    {
        GenericResultData<ProxyConfig> allowed() => GenericResult.CreateSuccess(entity);
        if (!entity.Enabled) return allowed();

        var conflicts = await _dbContext.ProxyConfigs
            .Where(x =>
                // Only compare vs enabled
                x.Enabled
                // Don't compare against self
                && entity.Id != x.Id
                // Subdomain must match for there to be a conflict
                && entity.Subdomain == x.Subdomain
                // Ports must be different, or at least of them a catch-all
                && (entity.Port == x.Port || entity.Port == null || x.Port == null)
            )
            .ToListAsync();

        if (conflicts.Any())
        {
            var conflictSummary = $"'{conflicts[0].Name}'";
            if (conflicts.Count > 1) conflictSummary = $"{conflictSummary} +{(conflicts.Count-1)} others.";
            return GenericResult.CreateError<ProxyConfig>($"Subdomain/port conflicts with another enabled config (e.g. {conflictSummary}).");
        }

        return allowed();
    }

    protected override IQueryable<ProxyConfig> OnGetSingle(DbSet<ProxyConfig> entities)
        => entities.Include(c => c.Authentications).ThenInclude(a => a.Conditions);

    protected override IQueryable<ProxyConfig> OnGetAllFull(DbSet<ProxyConfig> entities)
        => entities.Include(c => c.Authentications).ThenInclude(a => a.Conditions);

    protected override IQueryable<ProxyConfig> OnGetSingleFull(DbSet<ProxyConfig> entities)
        => entities.Include(c => c.Authentications).ThenInclude(a => a.Conditions);
}
