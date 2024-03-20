using Microsoft.AspNetCore.Mvc;
using QoDL.Toolkit.Web.Core.Utils;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class GlobalVariablesController(ApplicationDbContext context) : EFCrudControllerBase<GlobalVariable>(context, () => context.GlobalVariables)
{
    public override async Task<GenericResultData<GlobalVariable>> CreateOrUpdateEntityAsync([FromBody] GlobalVariable entity)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<GlobalVariable>(ModelState);

        entity.LastUpdatedAtUtc = DateTime.UtcNow;
        entity.LastUpdatedBy = HttpContext.User?.Identity?.Name ?? "unknown";
        entity.LastUpdatedSourceIP = TKRequestUtils.GetIPAddress(HttpContext);

        var isNew = entity.Id == Guid.Empty;
        var result = await base.CreateOrUpdateEntityAsync(entity);
        if (result.Success)
        {
            _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext,
                    isNew
                    ? $"Created new global variable {AdminAuditLogEntry.Placeholder_GlobalVariable}"
                    : $"Updated global variable {AdminAuditLogEntry.Placeholder_GlobalVariable}")
                    .SetRelatedGlobalVariable(result.Data?.Id, result.Data?.Name)
                );
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }

    public override async Task<GenericResult> DeleteEntityAsync([FromRoute] Guid entityId)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<GlobalVariable>(ModelState);

        var entity = await GetEntityAsync(entityId);
        var result = await base.DeleteEntityAsync(entityId);
        if (result.Success)
        {
            _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext, $"Deleted global variable {AdminAuditLogEntry.Placeholder_GlobalVariable}")
                    .SetRelatedGlobalVariable(entityId, entity?.Data?.Name)
                );
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }
}
