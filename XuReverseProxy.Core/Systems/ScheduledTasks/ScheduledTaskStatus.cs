using System.Text.Json.Serialization;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Systems.ScheduledTasks;

[GenerateFrontendModel]
public class ScheduledTaskStatus
{
    [JsonIgnore]
    public Type JobType { get; set; } = typeof(ScheduledTaskStatus);
    public string JobTypeName => JobType.Name;

    public bool IsRunning { get; set; }
    public bool Failed { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset LastStartedAt { get; set; }
    public DateTimeOffset? StoppedAt { get; set; }
}
