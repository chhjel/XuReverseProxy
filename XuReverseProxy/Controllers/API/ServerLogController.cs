using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XuReverseProxy.Core.Logging;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class ServerLogController : Controller
{
    [HttpGet]
    public List<MemoryLogger.LoggedEvent> GetLog() => MemoryLogger.GetEvents();
}
