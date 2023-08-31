using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Controllers.API;

public class ServerConfigController : EFCrudControllerBase<RuntimeServerConfigItem>
{
    public ServerConfigController(ApplicationDbContext context)
        : base(context, () => context.RuntimeServerConfigItems)
    {
    }
}
