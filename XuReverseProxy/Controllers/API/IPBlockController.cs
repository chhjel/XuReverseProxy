using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Services;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class IPBlockController(ApplicationDbContext context, IIPBlockService ipBlockService) : EFCrudControllerBase<BlockedIpData>(context, () => context.BlockedIpDatas)
{
    [HttpPost("IsIPBlocked")]
    public async Task<bool> IsIPBlockedAsync([FromBody] string ip)
        => await ipBlockService.IsIPBlockedAsync(ip);

    [HttpPost("GetMatchingBlockedIpDataFor")]
    public async Task<BlockedIpData?> GetMatchingBlockedIpDataForAsync([FromBody] string ip)
        => await ipBlockService.GetMatchingBlockedIpDataForAsync(ip, allowDisabled: false);

    [HttpPost("BlockIP")]
    public async Task<BlockedIpData> BlockIPAsync([FromBody] BlockIPRequestModel request)
        => await ipBlockService.BlockIPAsync(request.IP, request.Note, request.RelatedClientId);
    [GenerateFrontendModel]
    public record BlockIPRequestModel(string IP, string Note, Guid? RelatedClientId);

    [HttpPost("BlockIPRegex")]
    public async Task<BlockedIpData> BlockIPRegexAsync([FromBody] BlockIPRegexRequestModel request)
        => await ipBlockService.BlockIPRegexAsync(request.IPRegex, request.Note, request.RelatedClientId);
    [GenerateFrontendModel]
    public record BlockIPRegexRequestModel(string IPRegex, string Note, Guid? RelatedClientId);

    [HttpPost("BlockIPCidrRange")]
    public async Task<BlockedIpData> BlockIPCidrRangeAsync([FromBody] BlockIPCidrRangeRequestModel request)
        => await ipBlockService.BlockIPCidrRangeAsync(request.IPCidr, request.Note, request.RelatedClientId);
    [GenerateFrontendModel]
    public record BlockIPCidrRangeRequestModel(string IPCidr, string Note, Guid? RelatedClientId);

    [HttpDelete("RemoveIPBlockById")]
    public async Task RemoveIPBlockByIdAsync([FromBody] RemoveIPBlockByIdRequestModel request)
        => await ipBlockService.RemoveIPBlockByIdAsync(request.Id);
    [GenerateFrontendModel]
    public record RemoveIPBlockByIdRequestModel(Guid Id);
}
