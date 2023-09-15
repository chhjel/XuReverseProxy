using Microsoft.AspNetCore.Mvc;
using QoDL.Toolkit.Core.Extensions;
using XuReverseProxy.Core.Logging;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class ServerConfigController : EFCrudControllerBase<RuntimeServerConfigItem>
{
    public ServerConfigController(ApplicationDbContext context)
        : base(context, () => context.RuntimeServerConfigItems)
    {
    }

    public override async Task<GenericResultData<List<RuntimeServerConfigItem>>> GetAllEntitiesAsync()
    {
        var result = await base.GetAllEntitiesAsync();
        result.Data?.Add(new() {
                Id = Guid.Empty,
                Key = nameof(RuntimeServerConfig.EnableMemoryLogging),
                Value = MemoryLogger.Enabled.ToString().ToLowerInvariant()
            });
        return result;
    }

    public override async Task<GenericResultData<RuntimeServerConfigItem>> CreateOrUpdateEntityAsync([FromBody] RuntimeServerConfigItem entity)
    {
        if (!ModelState.IsValid) return GenericResult.CreateError<RuntimeServerConfigItem>(ModelState);

        if (entity.Key == nameof(RuntimeServerConfig.EnableMemoryLogging))
        {
            MemoryLogger.Enabled = entity.Value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
            return GenericResult.CreateSuccess(new RuntimeServerConfigItem());
        }

        var result = await base.CreateOrUpdateEntityAsync(entity);
        if (result.Success)
        {
            _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(Request.HttpContext, $"Updated config {entity.Key} = '{entity.Value?.LimitMaxLength(10)}'"));
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }
}
