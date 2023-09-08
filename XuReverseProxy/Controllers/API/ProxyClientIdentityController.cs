﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;

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

    protected override IQueryable<ProxyClientIdentity> OnGetSingle(DbSet<ProxyClientIdentity> entities)
        => entities.Include(i => i.SolvedChallenges).Include(i => i.Data);

    protected override IQueryable<ProxyClientIdentity> OnGetAllFull(DbSet<ProxyClientIdentity> entities)
        => entities.Include(i => i.SolvedChallenges).Include(i => i.Data);

    protected override IQueryable<ProxyClientIdentity> OnGetSingleFull(DbSet<ProxyClientIdentity> entities)
        => entities.Include(i => i.SolvedChallenges).Include(i => i.Data);
}
