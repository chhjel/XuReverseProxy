using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class Base32UtilController : Controller
{
    /// <summary>
    /// Get all entities.
    /// </summary>
    [HttpGet("createSecret")]
    public string CreateSecret() => TotpUtils.GenerateNewKey();
}
