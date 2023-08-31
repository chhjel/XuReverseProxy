using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class ProxyAuthenticationConditionController : EFCrudControllerBase<ProxyAuthenticationCondition>
{
    public ProxyAuthenticationConditionController(ApplicationDbContext context)
        : base(context,
            () => context.ProxyAuthenticationConditions)
    {
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
}
