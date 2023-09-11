using Microsoft.AspNetCore.Mvc;
using QoDL.Toolkit.Core.Extensions;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class ServerConfigController : EFCrudControllerBase<RuntimeServerConfigItem>
{
    public ServerConfigController(ApplicationDbContext context)
        : base(context, () => context.RuntimeServerConfigItems)
    {
    }

    public override async Task<GenericResultData<RuntimeServerConfigItem>> CreateOrUpdateEntityAsync([FromBody] RuntimeServerConfigItem entity)
    {
        var result = await base.CreateOrUpdateEntityAsync(entity);
        if (result.Success)
        {
            _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext, $"Updated config {entity.Key} = '{entity.Value?.LimitMaxLength(10)}'"));
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }
}
