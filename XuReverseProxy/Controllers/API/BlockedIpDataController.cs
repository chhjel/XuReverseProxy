using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Controllers.API;

public class BlockedIpDataController : EFCrudControllerBase<BlockedIpData>
{
    public BlockedIpDataController(ApplicationDbContext context)
        : base(context,
            () => context.BlockedIpDatas)
    {
    }
}
