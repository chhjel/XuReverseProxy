using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Controllers.API;

public class ProxyConfigController : EFCrudControllerBase<ProxyConfig>
{
    public ProxyConfigController(ApplicationDbContext context)
        : base(context, () => context.ProxyConfigs)
    {
    }

    protected override IQueryable<ProxyConfig> OnGetSingle(DbSet<ProxyConfig> entities)
        => entities.Include(c => c.Authentications).ThenInclude(a => a.Conditions);
}
