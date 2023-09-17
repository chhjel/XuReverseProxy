using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Controllers.API;

public class NotificationRuleController : EFCrudControllerBase<NotificationRule>
{
    public NotificationRuleController(ApplicationDbContext context, Func<DbSet<NotificationRule>> entities)
        : base(context, entities)
    {
    }
}
