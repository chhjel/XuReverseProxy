using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Systems.ScheduledTasks;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class ScheduledTasksController : Controller
{
    private readonly SchedulerHostedService _schedulerHostedService;
    private readonly IEnumerable<IScheduledTask> _scheduledTasks;

    public ScheduledTasksController(SchedulerHostedService schedulerHostedService, IEnumerable<IScheduledTask> scheduledTasks)
    {
        _schedulerHostedService = schedulerHostedService;
        _scheduledTasks = scheduledTasks;
    }

    [HttpGet]
    public List<ScheduledTaskViewModel> GetTasksDetails()
    {
        var models = new List<ScheduledTaskViewModel>();
        var statuses = _schedulerHostedService.Statuses;
        var results = _schedulerHostedService.LatestResults;
        foreach (var task in _scheduledTasks)
        {
            statuses.TryGetValue(task.GetType(), out var status);
            results.TryGetValue(task.GetType(), out var result);
            models.Add(new(task.Name, task.Description, status, result));
        }
        return models;
    }
    [GenerateFrontendModel]
    public record ScheduledTaskViewModel(string Name, string Description, ScheduledTaskStatus? Status = null, ScheduledTaskResult? Result = null);
}
