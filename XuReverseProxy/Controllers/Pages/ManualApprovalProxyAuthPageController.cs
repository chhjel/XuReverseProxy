﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Web.Core.Utils;
using XuReverseProxy.Attributes;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.ProxyAuthentication.Challenges;
using XuReverseProxy.Core.Services;
using XuReverseProxy.Models.ViewModels.Pages;

namespace XuReverseProxy.Controllers.Pages;

/// <summary>
/// Controller used for manually approving the proxy auth challenge <see cref="ProxyChallengeTypeManualApproval"/>.
/// </summary>
[Route("proxyAuth/approve/[action]")]
public class ManualApprovalProxyAuthPageController(IProxyClientIdentityService proxyClientIdentityService,
    ApplicationDbContext dbContext, IIPLookupService ipLookupService, IOptionsMonitor<ServerConfig> serverConfig,
    IProxyChallengeService proxyChallengeService, IIPBlockService iPBlockService, IConditionChecker conditionChecker) : Controller
{
    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpGet("/proxyAuth/approve/{clientIdentityId}/{proxyConfigId}/{authenticationId}/{solvedId}")]
    public async Task<IActionResult> Index([FromRoute] Guid clientIdentityId, [FromRoute] Guid proxyConfigId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId)
    {
        ViewBag.PageTitle = "Manual Approval";

        var client = await proxyClientIdentityService.GetProxyClientIdentityAsync(clientIdentityId);
        if (client == null) return NotFound();

        var config = await dbContext.ProxyConfigs
            .Include(x => x.ProxyConditions)
            .Include(x => x.Authentications)
            .FirstOrDefaultAsync(x => x.Id == proxyConfigId);
        if (config == null) return NotFound();

        var auth = await dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId);
        if (auth == null) return NotFound();

        var challengeDatas = dbContext.ProxyClientIdentityDatas
            .Where(x => x.IdentityId == clientIdentityId && x.AuthenticationId == authenticationId)
            .ToArray();

        var isApproved = await proxyChallengeService.IsChallengeSolvedAsync(client.Id, auth);
        var requestedAtTicks = challengeDatas.FirstOrDefault(x => x.Key.EndsWith(ProxyChallengeTypeManualApproval.DataKeyRequestedAt))?.Value ?? "0";
        var requestedAt = new DateTime(long.Parse(requestedAtTicks), DateTimeKind.Utc);
        var easyCode = challengeDatas.FirstOrDefault(x => x.Key.EndsWith(ProxyChallengeTypeManualApproval.DataKeyEasyCode))?.Value;
        var challengeData = new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.CurrentChallengeDataFrontendModel
        {
            EasyCode = easyCode,
            RequestedAt = requestedAt
        };

        var rawIp = TKRequestUtils.GetIPAddress(Request.HttpContext);
        var selfIpData = TKIPAddressUtils.ParseIP(rawIp, acceptLocalhostString: true);

        IPLookupResult? ipLocation = null;
#if DEBUG
        client.IP = "64.137.64.74"; // IP to test during dev
#endif
        if (!string.IsNullOrWhiteSpace(client.IP) && !client.IP.Equals("localhost", StringComparison.Ordinal))
        {
            ipLocation = await ipLookupService.LookupIPAsync(client.IP);
        }

        var url = serverConfig.CurrentValue.Domain.GetDomain(config.Subdomain);

        var clientData = new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.ClientDataFrontendModel
        {
            Id = client.Id,
            IP = client.IP,
            IPLocation = ipLocation,
            UserAgent = client.UserAgent,
            Note = client.Note,
            Blocked = client.Blocked,
            BlockedAtUtc = client.BlockedAtUtc,
            BlockedMessage = client.BlockedMessage,
            CreatedAtUtc = client.CreatedAtUtc,
            LastAccessedAtUtc = client.LastAccessedAtUtc,
            LastAttemptedAccessedAtUtc = client.LastAttemptedAccessedAtUtc
        };

        var allChallengeData = new List<ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.ChallengeDataFrontendModel>();
        var conditionContext = conditionChecker.CreateContext();
        foreach (var challenge in config.Authentications)
        {
            var conditionsData = await proxyChallengeService.GetChallengeRequirementDataAsync(challenge.Id, conditionContext);
            var solvedData = await proxyChallengeService.GetSolvedChallengeSolvedDataAsync(client.Id, challenge.Id, challenge.SolvedId, challenge.SolvedDuration);
            allChallengeData.Add(new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.ChallengeDataFrontendModel
            {
                Type = challenge.ChallengeTypeId,
                SolvedDuration = challenge.SolvedDuration,
                Solved = solvedData != null,
                SolvedAtUtc = solvedData?.SolvedAtUtc,
                ConditionsNotMet = conditionsData.Any(x => !x.Passed)
            });
        }

        var clientIPIsBlocked = false;
        Guid? clientIpBlockId = null;
        var canUnblockIP = false;
        var ipBlockType = BlockedIpDataType.None;
        if (!string.IsNullOrWhiteSpace(client.IP))
        {
            var blockedIpData = await iPBlockService.GetMatchingBlockedIpDataForAsync(client.IP, allowDisabled: false);
            clientIPIsBlocked = blockedIpData != null;
            clientIpBlockId = blockedIpData?.Id;
            canUnblockIP = !string.IsNullOrWhiteSpace(blockedIpData?.IP);
            ipBlockType = blockedIpData?.Type ?? BlockedIpDataType.None;
        }

        return View(new ManualApprovalProxyAuthPageViewModel
        {
            FrontendModel = new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel
            {
                AuthenticationId = authenticationId,
                SolvedId = solvedId,
                IsApproved = isApproved,
                IsLoggedIn = User.Identity?.IsAuthenticated == true,
                Url = url,
                Client = clientData,
                ClientIPIsBlocked = clientIPIsBlocked,
                ClientIPBlockId = clientIpBlockId,
                SelfIP = selfIpData?.IP,
                CanUnblockIP = canUnblockIP,
                IPBlockType = ipBlockType,
                CurrentChallengeData = challengeData,
                AllChallengeData = allChallengeData,
                ProxyConfig = new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.ProxyConfigFrontendModel
                {
                    Name = config.Name,
                    ChallengeTitle = config.ChallengeTitle,
                    Enabled = config.Enabled,
                    Subdomain = config.Subdomain,
                    Port = config.Port,
                    Destination = config.DestinationPrefix
                }
            }
        });
    }

    #region API
    // This separate API is needed for manual approval to bypass auth if enabled,
    // using the guid parameters as some security instead.

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/setclientnote")]
    public async Task<IActionResult> SetClientNote([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId,
        [FromBody] SetClientNoteFromManualApprovalRequestMessage request)
    {
        if (!ModelState.IsValid) return Json(false);

        // Ensure id's are valid
        var auth = await dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return Json(false);

        var success = await proxyClientIdentityService.SetClientNoteAsync(clientIdentityId, request.Note);
        return Json(success);
    }
    [GenerateFrontendModel]
    public record SetClientNoteFromManualApprovalRequestMessage(string Note);

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/approve")]
    public async Task<IActionResult> Approve([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId)
    {
        var success = await proxyChallengeService.SetChallengeSolvedAsync(clientIdentityId, authenticationId, solvedId);
        return Json(success);
    }

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/unapprove")]
    public async Task<IActionResult> Unapprove([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId)
    {
        var success = await proxyChallengeService.SetChallengeUnsolvedAsync(clientIdentityId, authenticationId, solvedId);
        return Json(success);
    }

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/block")]
    public async Task<IActionResult> SetClientBlocked([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId,
        [FromBody] SetClientBlockedFromManualApprovalRequestMessage request)
    {
        if (!ModelState.IsValid) return Json(false);

        // Ensure id's are valid
        var auth = await dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return Json(false);

        var success = false;
        if (request.Blocked)
            success = await proxyClientIdentityService.BlockIdentityAsync(clientIdentityId, request.Message);
        else
            success = await proxyClientIdentityService.UnBlockIdentityAsync(clientIdentityId);

        return Json(success);
    }
    [GenerateFrontendModel]
    public record SetClientBlockedFromManualApprovalRequestMessage(bool Blocked, string Message);

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/blockip")]
    public async Task<IActionResult> SetClientIPBlocked([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId,
        [FromBody] SetClientIPBlockedFromManualApprovalRequestMessage request)
    {
        if (!ModelState.IsValid) return Json(false);

        // Ensure id's are valid
        var auth = await dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return Json(false);
        var client = await proxyClientIdentityService.GetProxyClientIdentityAsync(clientIdentityId);
        if (client == null) return Json(false);

        var data = await iPBlockService.BlockIPAsync(request.IP, request.Note, clientIdentityId);

        return Json(data.Id);
    }
    [GenerateFrontendModel]
    public record SetClientIPBlockedFromManualApprovalRequestMessage(string IP, string Note);

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/removeipblock")]
    public async Task<IActionResult> RemoveIpBlock([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId,
        [FromBody] RemoveIPBlockFromManualApprovalRequestMessage request)
    {
        if (!ModelState.IsValid) return Json(false);

        // Ensure id's are valid
        var auth = await dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return Json(false);
        var client = await proxyClientIdentityService.GetProxyClientIdentityAsync(clientIdentityId);
        if (client == null) return Json(false);

        await iPBlockService.RemoveIPBlockByIdAsync(request.IPBlockId);

        return Json(true);
    }
    [GenerateFrontendModel]
    public record RemoveIPBlockFromManualApprovalRequestMessage(Guid IPBlockId);
    #endregion
}
