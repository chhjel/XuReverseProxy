using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Controllers.API;

public class HtmlTemplatesController(ApplicationDbContext context) 
    : EFCrudControllerBase<HtmlTemplate>(context, () => context.HtmlTemplates)
{
}
