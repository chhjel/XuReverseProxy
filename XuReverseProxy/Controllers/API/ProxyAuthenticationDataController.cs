using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class ProxyAuthenticationDataController : EFCrudControllerBase<ProxyAuthenticationData>
{
    public ProxyAuthenticationDataController(ApplicationDbContext context)
        : base(context, () => context.ProxyAuthenticationDatas)
    {
    }

    [HttpGet("fromConfig/{configId}")]
    public async Task<GenericResultData<List<ProxyAuthenticationData>>> GetFromConfigAsync([FromRoute] Guid configId)
    {
        try
        {
            var items = await _entities()
                .Include(c => c.Conditions)
                .Where(x => x.ProxyConfigId == configId)
                .ToListAsync();
            return GenericResult.CreateSuccess(items);
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError<List<ProxyAuthenticationData>>(ex.Message);
        }
    }

    [HttpPost("updateOrder")]
    public async Task<GenericResult> UpdateAuthOrdersAsync([FromBody] IList<ProxyAuthenticationDataOrderData> items)
    {
        try
        {
            var orderById = items.ToDictionary(x => x.AuthId, x => x.Order);
            var ids = items.Select(x => x.AuthId).ToHashSet();
            var auths = _dbContext.ProxyAuthenticationDatas.Where(x => ids.Contains(x.Id));
            foreach (var auth in auths)
            {
                auth.Order = orderById[auth.Id];
            }
            await _dbContext.SaveChangesAsync();
            return GenericResult.CreateSuccess();
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError(ex.Message);
        }
    }
    [GenerateFrontendModel]
    public record ProxyAuthenticationDataOrderData(Guid AuthId, int Order);

    protected override IQueryable<ProxyAuthenticationData> OnGetSingle(DbSet<ProxyAuthenticationData> entities)
        => entities.Include(i => i.Conditions);

    protected override IQueryable<ProxyAuthenticationData> OnGetAllFull(DbSet<ProxyAuthenticationData> entities)
        => entities.Include(i => i.Conditions);

    protected override IQueryable<ProxyAuthenticationData> OnGetSingleFull(DbSet<ProxyAuthenticationData> entities)
        => entities.Include(i => i.Conditions);
}
