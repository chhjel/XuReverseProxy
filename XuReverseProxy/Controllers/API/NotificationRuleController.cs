using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class NotificationRuleController(ApplicationDbContext context) : EFCrudControllerBase<NotificationRule>(context, () => context.NotificationRules)
{
    protected override Task<GenericResultData<NotificationRule>> ValidateEntityAsync(NotificationRule entity)
    {
        _dbContext.Entry(entity).Property(x => x.LastNotifiedAtUtc).IsModified = false;
        _dbContext.Entry(entity).Property(x => x.LastNotifyResult).IsModified = false;
        return base.ValidateEntityAsync(entity);
    }
}
