using System.Web;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.Services;

public interface INotificationService
{
    Task TryNotifyEvent(NotificationTrigger trigger, params IProvidesPlaceholders?[] placeholderProviders);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public NotificationService(ApplicationDbContext dbContext, IHttpClientFactory httpClientFactory)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    public async Task TryNotifyEvent(NotificationTrigger trigger, params IProvidesPlaceholders?[] placeholderProviders)
    {
        var now = DateTime.UtcNow;
        var matchingRules = _dbContext.NotificationRules.Where(x => x.Enabled
            && x.TriggerType == trigger
            && (x.Cooldown == null || x.LastNotifiedAtUtc == null || (now - x.LastNotifiedAtUtc) > x.Cooldown));
        
        foreach (var rule in matchingRules)
        {
            rule.LastNotifiedAtUtc = DateTime.UtcNow;
            rule.LastNotifyResult = "Notified";

            await NotifyAsync(rule, placeholderProviders);
        }
    }

    private async Task NotifyAsync(NotificationRule rule, IProvidesPlaceholders?[] placeholderProviders)
    {
        if (rule.AlertType == NotificationAlertType.WebHook)
            await NotifyWebHookAsync(rule, placeholderProviders);
        else
            throw new NotImplementedException($"Alert type '{rule.AlertType}' not implemented.");
    }

    private async Task NotifyWebHookAsync(NotificationRule rule, IProvidesPlaceholders?[] placeholderProviders)
    {
        if (string.IsNullOrWhiteSpace(rule.WebHookUrl)) return;

        var method = HttpMethod.Get;
        string? url = string.Empty;
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            url = PlaceholderUtils.ResolvePlaceholders(rule.WebHookUrl, transformer: HttpUtility.UrlEncode, placeholderProviders);

            if (!string.IsNullOrWhiteSpace(rule.WebHookMethod)) method = new HttpMethod(rule.WebHookMethod);

            var httpRequestMessage = new HttpRequestMessage(method, url);
            await httpClient.SendAsync(httpRequestMessage);

            rule.LastNotifyResult = $"Sent {method.Method} request to '{url}'.";
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            rule.LastNotifyResult = $"Exception while attempting to send {method.Method} request to '{url}': {ex}";
        }
        finally
        {
            rule.LastNotifiedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
}
