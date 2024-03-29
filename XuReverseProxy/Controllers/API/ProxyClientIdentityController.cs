﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Extensions;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Services;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class ProxyClientIdentityController(ApplicationDbContext context, IProxyClientIdentityService proxyClientIdentityService) : EFCrudControllerBase<ProxyClientIdentity>(context, () => context.ProxyClientIdentities)
{
    [HttpPost("paged")]
    public async Task<PaginatedResult<ProxyClientIdentity>> GetPagedAsync([FromBody] ProxyClientIdentitiesPagedRequestModel request)
    {
        var query = _dbContext.ProxyClientIdentities.AsQueryable();

        // Filter
        if (!string.IsNullOrWhiteSpace(request.Filter))
        {
            query = query.Where(x =>
                (x.Note != null && x.Note.ToLower().Contains(request.Filter.ToLower()))
                || (x.UserAgent != null && x.UserAgent.ToLower().Contains(request.Filter.ToLower()))
                || (x.IP != null && x.IP.ToLower().Contains(request.Filter.ToLower()))
            );
        }
        if (!string.IsNullOrWhiteSpace(request.IP))
        {
            query = query.Where(x => x.IP != null && x.IP.ToLower() == request.IP.ToLower());
        }
        if (request.NotId != null)
        {
            query = query.Where(x => x.Id != request.NotId);
        }

        if (request.HasAccessToProxyConfigId != null)
        {
            var proxyAuthIds = _dbContext.ProxyAuthenticationDatas
                .Where(x => x.ProxyConfigId == request.HasAccessToProxyConfigId.Value)
                .Select(x => x.Id)
                .ToHashSet();

            // note: access might be expired, to improve later
            query = query.Where(identity =>
                proxyAuthIds.All(authId => 
                    identity.SolvedChallenges.Any(d => 
                        d.AuthenticationId == authId
                    )
                )
            );
        }

        var totalCount = await query.CountAsync();

        // Sort
        if (request.SortBy == ProxyClientsSortBy.Created) query = query.SortByWithToggledDirection(x => x.CreatedAtUtc, request.SortDescending);
        else if (request.SortBy == ProxyClientsSortBy.LastAccessed) query = query.SortByWithToggledDirection(x => x.LastAccessedAtUtc, request.SortDescending);
        else if (request.SortBy == ProxyClientsSortBy.LastAttemptedAccessed) query = query.SortByWithToggledDirection(x => x.LastAttemptedAccessedAtUtc, request.SortDescending);
        else if (request.SortBy == ProxyClientsSortBy.Note) query = query.SortByWithToggledDirection(x => x.Note, request.SortDescending);
        else if (request.SortBy == ProxyClientsSortBy.IP) query = query.SortByWithToggledDirection(x => x.IP, request.SortDescending);
        else if (request.SortBy == ProxyClientsSortBy.Status) query = query.SortByWithToggledDirection(x => x.Blocked, request.SortDescending);
        else if (request.SortBy == ProxyClientsSortBy.UserAgent) query = query.SortByWithToggledDirection(x => x.UserAgent, request.SortDescending);
        else query = query.SortByWithToggledDirection(x => x.CreatedAtUtc, request.SortDescending);

        query = query
            .Skip(request.PageIndex * request.PageSize)
            .Take(request.PageSize);

        var pageItems = await query.ToListAsync();
        return new()
        {
            PageItems = pageItems,
            TotalItemCount = totalCount
        };
    }
    [GenerateFrontendModel]
    public record ProxyClientIdentitiesPagedRequestModel(int PageIndex, int PageSize, 
        ProxyClientsSortBy? SortBy = null, bool SortDescending = true, 
        string? Filter = null, string? IP = null, Guid? NotId = null, Guid? HasAccessToProxyConfigId = null);
    [GenerateFrontendModel]
    public enum ProxyClientsSortBy { Created, LastAccessed, LastAttemptedAccessed, Note, IP, Status, UserAgent }

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
            await proxyClientIdentityService.SetClientNoteAsync(request.ClientId, request.Note);
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
            if (request.Blocked) await proxyClientIdentityService.BlockIdentityAsync(request.ClientId, request.Message);
            else await proxyClientIdentityService.UnBlockIdentityAsync(request.ClientId);
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
