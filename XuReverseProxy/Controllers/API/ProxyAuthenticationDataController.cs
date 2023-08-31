using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class ProxyAuthenticationDataController : EFCrudControllerBase<ProxyAuthenticationData>
{
    public ProxyAuthenticationDataController(ApplicationDbContext context)
        : base(context, () => context.ProxyAuthenticationDatas)
    {
    }

    protected override IQueryable<ProxyAuthenticationData> OnGetSingle(DbSet<ProxyAuthenticationData> entities)
        => entities.Include(i => i.Conditions);

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
}
