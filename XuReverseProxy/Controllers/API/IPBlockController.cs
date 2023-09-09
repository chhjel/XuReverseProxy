using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoDL.Toolkit.Core.Util;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Services;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class IPBlockController : Controller
{
    private readonly IIPBlockService _ipBlockService;

    public IPBlockController(IIPBlockService ipBlockService)
    {
        _ipBlockService = ipBlockService;
    }

    [HttpPost("IsIPBlocked")]
    public async Task<bool> IsIPBlockedAsync([FromBody] string ip)
        => await _ipBlockService.IsIPBlockedAsync(ip);

    [HttpPost("GetMatchingBlockedIpDataFor")]
    public async Task<BlockedIpData?> GetMatchingBlockedIpDataForAsync([FromBody] string ip)
        => await _ipBlockService.GetMatchingBlockedIpDataForAsync(ip);

    [HttpPost("BlockIP")]
    public async Task<BlockedIpData> BlockIPAsync([FromBody] BlockIPRequestModel request)
        => await _ipBlockService.BlockIPAsync(request.IP, request.Note, request.RelatedClientId);
    [GenerateFrontendModel]
    public record BlockIPRequestModel(string IP, string Note, Guid? RelatedClientId);

    [HttpPost("BlockIPRegex")]
    public async Task<BlockedIpData> BlockIPRegexAsync([FromBody] BlockIPRegexRequestModel request)
        => await _ipBlockService.BlockIPRegexAsync(request.IPRegex, request.Note, request.RelatedClientId);
    [GenerateFrontendModel]
    public record BlockIPRegexRequestModel(string IPRegex, string Note, Guid? RelatedClientId);

    [HttpPost("BlockIPCidrRange")]
    public async Task<BlockedIpData> BlockIPCidrRangeAsync([FromBody] BlockIPCidrRangeRequestModel request)
        => await _ipBlockService.BlockIPCidrRangeAsync(request.IPCidr, request.Note, request.RelatedClientId);
    [GenerateFrontendModel]
    public record BlockIPCidrRangeRequestModel(string IPCidr, string Note, Guid? RelatedClientId);

    [HttpDelete("RemoveIPBlockById")]
    public async Task RemoveIPBlockByIdAsync([FromBody] RemoveIPBlockByIdRequestModel request)
        => await _ipBlockService.RemoveIPBlockByIdAsync(request.Id);
    [GenerateFrontendModel]
    public record RemoveIPBlockByIdRequestModel(Guid Id);

    [HttpPost("IsIPInCidrRange")]
    public bool IsIPInCidrRange([FromBody] TestCidrRangeRequestModel request)
        => TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(request.IP, request.IPCidr);
    [GenerateFrontendModel]
    public record TestCidrRangeRequestModel(string IP, string IPCidr);
}
