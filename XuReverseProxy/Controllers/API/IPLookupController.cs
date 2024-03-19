using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XuReverseProxy.Core.Services;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class IPLookupController(IIPLookupService ipLookupService) : Controller
{
    [HttpPost("lookup")]
    public async Task<IPLookupResult?> LookupAsync([FromBody] string ip)
        => await ipLookupService.LookupIPAsync(ip);
}
