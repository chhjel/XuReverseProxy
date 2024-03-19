using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XuReverseProxy.Core.Logging;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class ServerLogController : Controller
{
    [HttpGet]
    public List<MemoryLogger.LoggedEvent> GetLog()
    {
        try
        {
            return MemoryLogger.GetEvents()
                .Select(x => new MemoryLogger.LoggedEvent(x.TimestampUtc, x.LogLevel, x.EventId, null, x.Message + (x.Exception != null ? $" - Exception: {x.Exception}" : null)))
                .ToList();
        }
        catch (Exception ex)
        {
            return
            [
                new MemoryLogger.LoggedEvent(DateTime.UtcNow, LogLevel.Critical, 0, null, "Failed to load log events."),
                new MemoryLogger.LoggedEvent(DateTime.UtcNow, LogLevel.Critical, 0, null, ex.ToString())
            ];
        }
    }
}
