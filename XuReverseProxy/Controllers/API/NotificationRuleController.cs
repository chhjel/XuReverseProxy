using Microsoft.EntityFrameworkCore;
using Polly;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

public class NotificationRuleController : EFCrudControllerBase<NotificationRule>
{
    public NotificationRuleController(ApplicationDbContext context)
        : base(context, () => context.NotificationRules)
    {
    }

    protected override Task<GenericResultData<NotificationRule>> ValidateEntityAsync(NotificationRule entity)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        _dbContext.Entry(entity).Property(x => x.LastNotifiedAtUtc).IsModified = false;
        _dbContext.Entry(entity).Property(x => x.LastNotifyResult).IsModified = false;
        return base.ValidateEntityAsync(entity);
    }
}
