using System.Text.Json.Serialization;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Systems.ScheduledTasks;

[GenerateFrontendModel]
public class ScheduledTaskResult
{
    [JsonIgnore]
    public Type JobType { get; set; } = typeof(ScheduledTaskResult);
    public string JobTypeName => JobType.Name;

    public bool Success { get; set; }
    public string? Result { get; set; }
    public Exception? Exception { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime StoppedAtUtc { get; set; }

    public ScheduledTaskResult SetResult(string? result)
    {
        Result = result;
        return this;
    }
}
