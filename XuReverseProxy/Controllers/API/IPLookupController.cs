using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XuReverseProxy.Core.Services;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class IPLookupController : Controller
{
    private readonly IIPLookupService _ipLookupService;

    public IPLookupController(IIPLookupService ipLookupService)
    {
        _ipLookupService = ipLookupService;
    }

    [HttpPost("lookup")]
    public async Task<IPLookupResult?> LookupAsync([FromBody] string ip)
        => await _ipLookupService.LookupIPAsync(ip);
}
