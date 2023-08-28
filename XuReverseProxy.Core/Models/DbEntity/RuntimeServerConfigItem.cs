namespace XuReverseProxy.Core.Models.DbEntity;

public class RuntimeServerConfigItem
{
    public Guid Id { get; set; }
    public DateTime LastUpdatedAtUtc { get; set; }
    public string? LastUpdatedBy { get; set; }
    public string? LastUpdatedSourceIP { get; set; }

    public string? Key { get; set; }
    public string? Value { get; set; }
}
