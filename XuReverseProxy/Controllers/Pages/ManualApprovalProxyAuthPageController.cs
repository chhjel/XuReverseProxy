using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
public class ManualApprovalProxyAuthPageController : Controller
{
    private readonly IProxyClientIdentityService _proxyClientIdentityService;
    private readonly ApplicationDbContext _dbContext;

    public ManualApprovalProxyAuthPageController(IProxyClientIdentityService proxyClientIdentityService, ApplicationDbContext dbContext)
    {
        _proxyClientIdentityService = proxyClientIdentityService;
        _dbContext = dbContext;
    }

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpGet("/proxyAuth/approve/{clientIdentityId}/{proxyConfigId}/{authenticationId}/{solvedId}")]
    public async Task<IActionResult> Index([FromRoute] Guid clientIdentityId, [FromRoute] Guid proxyConfigId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId)
    {
        ViewBag.PageTitle = "Manual Approval";

        var client = await _proxyClientIdentityService.GetProxyClientIdentityAsync(clientIdentityId);
        if (client == null) return NotFound();

        var config = await _dbContext.ProxyConfigs.Include(x => x.Authentications).FirstOrDefaultAsync(x => x.Id == proxyConfigId);
        if (config == null) return NotFound();

        var auth = await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId);
        if (auth == null) return NotFound();

        var challengeDatas = _dbContext.ProxyClientIdentityDatas
            .Where(x => x.IdentityId == clientIdentityId && x.AuthenticationId == authenticationId)
            .ToArray();

        var requestedAtTicks = challengeDatas.FirstOrDefault(x => x.Key.EndsWith(ProxyChallengeTypeManualApproval.DataKeyRequestedAt))?.Value ?? "0";
        var requestedAt = new DateTime(long.Parse(requestedAtTicks), DateTimeKind.Utc);
        var easyCode = challengeDatas.FirstOrDefault(x => x.Key.EndsWith(ProxyChallengeTypeManualApproval.DataKeyEasyCode))?.Value;
        var challengeData = new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.CurrentChallengeDataFrontendModel
        {
            EasyCode = easyCode,
            RequestedAt = requestedAt
        };

        var clientData = new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.ClientDataFrontendModel
        {
            Id = client.Id,
            IP = client.IP,
            UserAgent = client.UserAgent,
            Blocked = client.Blocked,
            BlockedAtUtc = client.BlockedAtUtc,
            BlockedMessage = client.BlockedMessage,
            CreatedAtUtc = client.CreatedAtUtc,
            LastAccessedAtUtc = client.LastAccessedAtUtc,
            LastAttemptedAccessedAtUtc = client.LastAttemptedAccessedAtUtc
        };

        var allChallengeData = new List<ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.ChallengeDataFrontendModel>();
        foreach (var challenge in config.Authentications)
        {
            var solvedData = await _proxyClientIdentityService.GetSolvedChallengeSolvedDataAsync(client.Id, challenge.Id, challenge.SolvedId, challenge.SolvedDuration);
            allChallengeData.Add(new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.ChallengeDataFrontendModel
            {
                Type = challenge.ChallengeTypeId,
                SolvedDuration = challenge.SolvedDuration,
                Solved = solvedData != null,
                SolvedAtUtc = solvedData?.SolvedAtUtc
            });
        }
        return View(new ManualApprovalProxyAuthPageViewModel
        {
            FrontendModel = new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel
            {
                AuthenticationId = authenticationId,
                SolvedId = solvedId,
                IsLoggedIn = User.Identity?.IsAuthenticated == true,
                Client = clientData,
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

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/approve")]
    public async Task<IActionResult> Approve([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId)
    {
        var success = await _proxyClientIdentityService.SetChallengeSolvedAsync(clientIdentityId, authenticationId, solvedId);
        return Json(success);
    }

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/unapprove")]
    public async Task<IActionResult> Unapprove([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId)
    {
        var success = await _proxyClientIdentityService.SetChallengeUnsolvedAsync(clientIdentityId, authenticationId, solvedId);
        return Json(success);
    }

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/block")]
    public async Task<IActionResult> SetClientBlocked([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId,
        [FromBody] SetClientBlockedFromManualApprovalRequestMessage request)
    {
        if (!ModelState.IsValid) return Json(false);

        // Ensure id's are valid
        var identity = await _proxyClientIdentityService.GetProxyClientIdentityAsync(clientIdentityId);
        if (identity == null) return Json(false);
        var auth = await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return Json(false);

        if (request.Blocked)
            await _proxyClientIdentityService.BlockIdentityAsync(clientIdentityId, request.Message);
        else
            await _proxyClientIdentityService.UnBlockIdentityAsync(clientIdentityId);

        return Json(true);
    }
    [GenerateFrontendModel]
    public record SetClientBlockedFromManualApprovalRequestMessage(bool Blocked, string Message);
}
