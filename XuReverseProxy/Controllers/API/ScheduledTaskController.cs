using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XuReverseProxy.Core.Systems.ScheduledTasks;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class ScheduledTasksController : Controller
{
    private readonly SchedulerHostedService _schedulerHostedService;

    public ScheduledTasksController(SchedulerHostedService schedulerHostedService)
    {
        _schedulerHostedService = schedulerHostedService;
    }

    [HttpGet("statuses")]
    public List<ScheduledTaskStatus> GetStatuses()
        => _schedulerHostedService.Statuses.Select(x => x.Value).ToList();

    [HttpGet("results")]
    public List<ScheduledTaskResult> GetLatestResults()
        => _schedulerHostedService.LatestResults.Select(x => x.Value).ToList();
}
