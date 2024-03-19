using Microsoft.EntityFrameworkCore;
using QoDL.Toolkit.Core.Util;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Controllers.API;

public class HtmlTemplatesController(ApplicationDbContext context) 
    : EFCrudControllerBase<HtmlTemplate>(context, () => context.HtmlTemplates)
{
    protected override IQueryable<HtmlTemplate> OnGetAll(DbSet<HtmlTemplate> entities)
    {
        var all = base.OnGetAll(entities);
        
        var defaults = HtmlTemplate.HtmlTemplateDefaults;
        var missing = defaults.Where(x => !all.Any(e => e.Type == x.Key)).ToArray();
        if (missing.Length > 0)
        {
            foreach(var item in missing)
            {
                TKAsyncUtils.RunSync(() => CreateOrUpdateEntityAsync(item.Value));
            }
            all = base.OnGetAll(entities);
        }

        return all;
    }
}
