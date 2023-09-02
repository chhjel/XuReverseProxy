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
