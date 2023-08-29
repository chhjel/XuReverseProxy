using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    private readonly IIPLookupService _ipLookupService;
    private readonly IOptionsMonitor<ServerConfig> _serverConfig;
    private readonly IProxyChallengeService _proxyChallengeService;

    public ManualApprovalProxyAuthPageController(IProxyClientIdentityService proxyClientIdentityService,
        ApplicationDbContext dbContext, IIPLookupService ipLookupService, IOptionsMonitor<ServerConfig> serverConfig, IProxyChallengeService proxyChallengeService)
    {
        _proxyClientIdentityService = proxyClientIdentityService;
        _dbContext = dbContext;
        _ipLookupService = ipLookupService;
        _serverConfig = serverConfig;
        _proxyChallengeService = proxyChallengeService;
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

        var isApproved = await _proxyChallengeService.IsChallengeSolvedAsync(client.Id, auth);
        var requestedAtTicks = challengeDatas.FirstOrDefault(x => x.Key.EndsWith(ProxyChallengeTypeManualApproval.DataKeyRequestedAt))?.Value ?? "0";
        var requestedAt = new DateTime(long.Parse(requestedAtTicks), DateTimeKind.Utc);
        var easyCode = challengeDatas.FirstOrDefault(x => x.Key.EndsWith(ProxyChallengeTypeManualApproval.DataKeyEasyCode))?.Value;
        var challengeData = new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.CurrentChallengeDataFrontendModel
        {
            EasyCode = easyCode,
            RequestedAt = requestedAt
        };

        IPLookupResult? ipLocation = null;
#if DEBUG
        client.IP = "64.137.64.74"; // IP to test during dev
#endif
        if (!string.IsNullOrWhiteSpace(client.IP) && !client.IP.Equals("localhost", StringComparison.Ordinal))
        {
            ipLocation = await _ipLookupService.LookupIPAsync(client.IP);
        }

        var url = _serverConfig.CurrentValue.Domain.GetDomain(config.Subdomain);

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
        foreach (var challenge in config.Authentications)
        {
            var conditionsData = _proxyChallengeService.GetChallengeRequirementData(challenge.Id);
            var solvedData = await _proxyChallengeService.GetSolvedChallengeSolvedDataAsync(client.Id, challenge.Id, challenge.SolvedId, challenge.SolvedDuration);
            allChallengeData.Add(new ManualApprovalProxyAuthPageViewModel.ManualApprovalProxyAuthPageFrontendModel.ChallengeDataFrontendModel
            {
                Type = challenge.ChallengeTypeId,
                SolvedDuration = challenge.SolvedDuration,
                Solved = solvedData != null,
                SolvedAtUtc = solvedData?.SolvedAtUtc,
                ConditionsNotMet = conditionsData.Any(x => !x.Passed)
            });
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
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/setclientnote")]
    public async Task<IActionResult> SetClientNote([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId,
        [FromBody] SetClientNoteFromManualApprovalRequestMessage request)
    {
        if (!ModelState.IsValid) return Json(false);

        // Ensure id's are valid
        var auth = await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return Json(false);

        var success = await _proxyClientIdentityService.SetClientNoteAsync(clientIdentityId, request.Note);
        return Json(success);
    }
    [GenerateFrontendModel]
    public record SetClientNoteFromManualApprovalRequestMessage(string Note);

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/approve")]
    public async Task<IActionResult> Approve([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId)
    {
        var success = await _proxyChallengeService.SetChallengeSolvedAsync(clientIdentityId, authenticationId, solvedId);
        return Json(success);
    }

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/unapprove")]
    public async Task<IActionResult> Unapprove([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId)
    {
        var success = await _proxyChallengeService.SetChallengeUnsolvedAsync(clientIdentityId, authenticationId, solvedId);
        return Json(success);
    }

    [AuthorizeIfEnabled(nameof(RuntimeServerConfig.EnableManualApprovalPageAuthentication))]
    [HttpPost("/proxyAuth/approve/{clientIdentityId}/{authenticationId}/{solvedId}/block")]
    public async Task<IActionResult> SetClientBlocked([FromRoute] Guid clientIdentityId, [FromRoute] Guid authenticationId, [FromRoute] Guid solvedId,
        [FromBody] SetClientBlockedFromManualApprovalRequestMessage request)
    {
        if (!ModelState.IsValid) return Json(false);

        // Ensure id's are valid
        var auth = await _dbContext.ProxyAuthenticationDatas.FirstOrDefaultAsync(x => x.Id == authenticationId && x.SolvedId == solvedId);
        if (auth == null) return Json(false);

        var success = false;
        if (request.Blocked)
            success = await _proxyClientIdentityService.BlockIdentityAsync(clientIdentityId, request.Message);
        else
            success = await _proxyClientIdentityService.UnBlockIdentityAsync(clientIdentityId);

        return Json(success);
    }
    [GenerateFrontendModel]
    public record SetClientBlockedFromManualApprovalRequestMessage(bool Blocked, string Message);
}
