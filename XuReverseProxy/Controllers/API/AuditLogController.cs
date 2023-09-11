using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class AuditLogController : Controller
{
    protected readonly ApplicationDbContext _dbContext;

    public AuditLogController(ApplicationDbContext context)
    {
        _dbContext = context;
    }

    [HttpPost("adminLog")]
    public async Task<PaginatedResult<AdminAuditLogEntry>> GetAdminAuditLogEntriesAsync([FromBody] GetAdminAuditLogEntriesRequestModel request)
    {
        var query = _dbContext.AdminAuditLogEntries.Where(x => x.TimestampUtc >= request.FromUtc && x.TimestampUtc <= request.ToUtc);
        if (request.AdminUserId != null) query = query.Where(x => x.AdminUserId == request.AdminUserId);
        if (request.ProxyConfigId != null) query = query.Where(x => x.RelatedProxyConfigId == request.ProxyConfigId);
        if (request.ClientId != null) query = query.Where(x => x.RelatedClientId == request.ClientId);
        var totalCount = await query.CountAsync();

        query = query
            .OrderByDescending(x => x.TimestampUtc)
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
    public record GetAdminAuditLogEntriesRequestModel(DateTime FromUtc, DateTime ToUtc, int PageIndex, int PageSize,
        Guid? AdminUserId, Guid? ProxyConfigId, Guid? ClientId);

    [HttpPost("clientLog")]
    public async Task<PaginatedResult<ClientAuditLogEntry>> GetClientAuditLogEntriesAsync([FromBody] GetClientAuditLogEntriesRequestModel request)
    {
        var query = _dbContext.ClientAuditLogEntries.Where(x => x.TimestampUtc >= request.FromUtc && x.TimestampUtc <= request.ToUtc);
        if (request.ClientId != null) query = query.Where(x => x.ClientId == request.ClientId);
        if (request.ProxyConfigId != null) query = query.Where(x => x.RelatedProxyConfigId == request.ProxyConfigId);
        var totalCount = await query.CountAsync();

        query = query
            .OrderByDescending(x => x.TimestampUtc)
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
    public record GetClientAuditLogEntriesRequestModel(DateTime FromUtc, DateTime ToUtc, int PageIndex, int PageSize,
        Guid? ProxyConfigId, Guid? ClientId);
}
