using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Services;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class ProxyClientIdentityController : EFCrudControllerBase<ProxyClientIdentity>
{
    private readonly IProxyClientIdentityService _proxyClientIdentityService;

    public ProxyClientIdentityController(ApplicationDbContext context, IProxyClientIdentityService proxyClientIdentityService)
        : base(context, () => context.ProxyClientIdentities)
    {
        _proxyClientIdentityService = proxyClientIdentityService;
    }

    // Needed to preserve hash after login
    [HttpGet("redirect/to-client-details/{clientid}")]
    public IActionResult RedirectToClientDetails([FromRoute] Guid clientid)
        => Redirect($"/#/client/{clientid}");

    [HttpPost("setNote")]
    public async Task<GenericResult> SetClientNoteAsync([FromBody] SetClientNoteRequestModel request)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError(ModelState);

        try
        {
            await _proxyClientIdentityService.SetClientNoteAsync(request.ClientId, request.Note);
            return GenericResult.CreateSuccess();
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError(ex.Message);
        }
    }
    [GenerateFrontendModel]
    public record SetClientNoteRequestModel(Guid ClientId, string Note);

    [HttpPost("setBlocked")]
    public async Task<GenericResult> SetClientBlockedAsync([FromBody] SetClientBlockedRequestModel request)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError(ModelState);

        try
        {
            if (request.Blocked) await _proxyClientIdentityService.BlockIdentityAsync(request.ClientId, request.Message);
            else await _proxyClientIdentityService.UnBlockIdentityAsync(request.ClientId);
            return GenericResult.CreateSuccess();
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError(ex.Message);
        }
    }
    [GenerateFrontendModel]
    public record SetClientBlockedRequestModel(Guid ClientId, bool Blocked, string Message);

    protected override IQueryable<ProxyClientIdentity> OnGetSingle(DbSet<ProxyClientIdentity> entities)
        => entities.Include(i => i.SolvedChallenges).Include(i => i.Data);

    protected override IQueryable<ProxyClientIdentity> OnGetAllFull(DbSet<ProxyClientIdentity> entities)
        => entities.Include(i => i.SolvedChallenges).Include(i => i.Data);

    protected override IQueryable<ProxyClientIdentity> OnGetSingleFull(DbSet<ProxyClientIdentity> entities)
        => entities.Include(i => i.SolvedChallenges).Include(i => i.Data);
}
