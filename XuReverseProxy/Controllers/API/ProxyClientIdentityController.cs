using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class ProxyClientIdentityController : EFCrudControllerBase<ProxyClientIdentity>
{
    public ProxyClientIdentityController(ApplicationDbContext context)
        : base(context, () => context.ProxyClientIdentities)
    {
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
            var client = await _dbContext.ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == request.ClientId);
            if (client == null) return GenericResult.CreateError("Client not found.");

            client.Note = request.Note;
            await _dbContext.SaveChangesAsync();
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
            var client = await _dbContext.ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == request.ClientId);
            if (client == null) return GenericResult.CreateError("Client not found.");

            if (!client.Blocked && request.Blocked) client.BlockedAtUtc = DateTime.UtcNow;
            else if (client.Blocked && !request.Blocked) client.BlockedAtUtc = null;
            client.Blocked = request.Blocked;
            client.BlockedMessage = request.Message;
            await _dbContext.SaveChangesAsync();
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
