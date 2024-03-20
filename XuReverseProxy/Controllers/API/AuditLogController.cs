using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Extensions;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class AuditLogController(ApplicationDbContext context) : Controller
{
    protected readonly ApplicationDbContext _dbContext = context;

    [HttpPost("adminLog")]
    public async Task<PaginatedResult<AdminAuditLogEntry>> GetAdminAuditLogEntriesAsync([FromBody] GetAdminAuditLogEntriesRequestModel request)
    {
        // Filter
        var query = _dbContext.AdminAuditLogEntries.Where(x => x.TimestampUtc >= request.FromUtc && x.TimestampUtc <= request.ToUtc);
        if (request.AdminUserId != null) query = query.Where(x => x.AdminUserId == request.AdminUserId);
        if (request.ProxyConfigId != null) query = query.Where(x => x.RelatedProxyConfigId == request.ProxyConfigId);
        if (request.ClientId != null) query = query.Where(x => x.RelatedClientId == request.ClientId);

        var totalCount = await query.CountAsync();

        // Sort
        if (request.SortBy == AdminAuditLogSortBy.Timestamp) query = query.SortByWithToggledDirection(x => x.TimestampUtc, request.SortDescending);
        else if (request.SortBy == AdminAuditLogSortBy.IP) query = query.SortByWithToggledDirection(x => x.IP, request.SortDescending);
        else if (request.SortBy == AdminAuditLogSortBy.Who) query = query.SortByWithToggledDirection(x => x.AdminUserId, request.SortDescending);
        else if (request.SortBy == AdminAuditLogSortBy.What) query = query.SortByWithToggledDirection(x => x.Action, request.SortDescending);
        else query = query.SortByWithToggledDirection(x => x.TimestampUtc, request.SortDescending);

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
    public record GetAdminAuditLogEntriesRequestModel(DateTime FromUtc, DateTime ToUtc, int PageIndex, int PageSize,
        Guid? AdminUserId, Guid? ProxyConfigId, Guid? ClientId, AdminAuditLogSortBy? SortBy = null, bool SortDescending = true);
    [GenerateFrontendModel]
    public enum AdminAuditLogSortBy { Timestamp, IP, Who, What }

    [HttpPost("clientLog")]
    public async Task<PaginatedResult<ClientAuditLogEntry>> GetClientAuditLogEntriesAsync([FromBody] GetClientAuditLogEntriesRequestModel request)
    {
        // Filter
        var query = _dbContext.ClientAuditLogEntries.Where(x => x.TimestampUtc >= request.FromUtc && x.TimestampUtc <= request.ToUtc);
        if (request.ClientId != null) query = query.Where(x => x.ClientId == request.ClientId);
        if (request.ProxyConfigId != null) query = query.Where(x => x.RelatedProxyConfigId == request.ProxyConfigId);

        var totalCount = await query.CountAsync();

        // Sort
        if (request.SortBy == ClientAuditLogSortBy.Timestamp) query = query.SortByWithToggledDirection(x => x.TimestampUtc, request.SortDescending);
        else if (request.SortBy == ClientAuditLogSortBy.IP) query = query.SortByWithToggledDirection(x => x.IP, request.SortDescending);
        else if (request.SortBy == ClientAuditLogSortBy.Who) query = query.SortByWithToggledDirection(x => x.ClientId, request.SortDescending);
        else if (request.SortBy == ClientAuditLogSortBy.What) query = query.SortByWithToggledDirection(x => x.Action, request.SortDescending);
        else query = query.SortByWithToggledDirection(x => x.TimestampUtc, request.SortDescending);

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
    public record GetClientAuditLogEntriesRequestModel(DateTime FromUtc, DateTime ToUtc, int PageIndex, int PageSize,
        Guid? ProxyConfigId, Guid? ClientId, ClientAuditLogSortBy? SortBy = null, bool SortDescending = true);
    [GenerateFrontendModel]
    public enum ClientAuditLogSortBy { Timestamp, IP, Who, What }
}
