using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class NotificationRule : IHasId
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool Enabled { get; set; }

    public NotificationTrigger TriggerType { get; set; }
    public NotificationAlertType AlertType { get; set; }

    public string? WebHookUrl { get; set; }
    public string? WebHookMethod { get; set; }
    public string? WebHookBody { get; set; }

    public TimeSpan? Cooldown { get; set; }

    public DateTime? LastNotifiedAtUtc { get; set; }
    public string? LastNotifyResult { get; set; }
}

[GenerateFrontendModel]
public enum NotificationTrigger
{
    AdminLoginSuccess = 0,
    AdminLoginFailed = 1,
    AdminRequests = 10,
    AdminSessionIPChanged = 11,
    NewClient = 20,
    ClientRequest = 30,
    ClientCompletedChallenge = 40
}

[GenerateFrontendModel]
public enum NotificationAlertType
{
    WebHook = 0,
}
