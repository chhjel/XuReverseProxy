using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Controllers.API;

public class ProxyClientIdentityController : EFCrudControllerBase<ProxyClientIdentity>
{
    public ProxyClientIdentityController(ApplicationDbContext context)
        : base(context, () => context.ProxyClientIdentities)
    {
    }

    protected override IQueryable<ProxyClientIdentity> OnGetSingle(DbSet<ProxyClientIdentity> entities)
        => entities.Include(i => i.SolvedChallenges).Include(i => i.Data);

    protected override IQueryable<ProxyClientIdentity> OnGetAllFull(DbSet<ProxyClientIdentity> entities)
        => entities.Include(i => i.SolvedChallenges).Include(i => i.Data);

    protected override IQueryable<ProxyClientIdentity> OnGetSingleFull(DbSet<ProxyClientIdentity> entities)
        => entities.Include(i => i.SolvedChallenges).Include(i => i.Data);
}
