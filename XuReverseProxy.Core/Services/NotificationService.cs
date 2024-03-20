using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Web;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Models.Common;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Services;

public interface INotificationService
{
    Task TryNotifyEvent(NotificationTrigger trigger, params IProvidesPlaceholders?[] placeholderProviders);
    Task TryNotifyEvent(NotificationTrigger trigger, Dictionary<string, string?>? placeholders, params IProvidesPlaceholders?[] placeholderProviders);
}

public class NotificationService(ApplicationDbContext dbContext, IMemoryCache memoryCache,
    ILogger<NotificationService> logger, RuntimeServerConfig runtimeServerConfig,
    IServiceScopeFactory serviceScopeFactory, IPlaceholderResolver placeholderResolver) : INotificationService
{
    public async Task TryNotifyEvent(NotificationTrigger trigger, params IProvidesPlaceholders?[] placeholderProviders)
        => await TryNotifyEvent(trigger, null, placeholderProviders);

    public async Task TryNotifyEvent(NotificationTrigger trigger, Dictionary<string, string?>? placeholders, params IProvidesPlaceholders?[] placeholderProviders)
    {
        if (!runtimeServerConfig.EnableNotifications) return;

        var now = DateTime.UtcNow;
        var matchingRules = (await dbContext.GetWithCacheAsync(x => x.NotificationRules))
            .Where(x => x.Enabled && x.TriggerType == trigger)
            .ToList();

        foreach (var rule in matchingRules)
        {
            if (await HandleRuleCooldown(rule, placeholders, placeholderProviders)) continue;
            
            // Notify async
            var _ = NotifyAsync(rule, placeholders, placeholderProviders, serviceScopeFactory, logger);
        }
    }

    /// <summary>
    /// Returns true if the rule is on cooldown. Sets cooldown if enabled.
    /// </summary>
    private async Task<bool> HandleRuleCooldown(NotificationRule rule, Dictionary<string, string?>? placeholders, IProvidesPlaceholders?[] placeholderProviders)
    {
        if (!(rule.Cooldown > TimeSpan.Zero)) return false;

        var distinctCacheKey = $"_notify_distinction_{rule.Id}";
        if (!string.IsNullOrWhiteSpace(rule.CooldownDistinctPattern))
        {
            var distinctPattern = rule.CooldownDistinctPattern;
            if (placeholders != null) distinctPattern = await placeholderResolver.ResolvePlaceholdersAsync(distinctPattern, placeholders: placeholders, placeholderProviders: placeholderProviders);
            distinctCacheKey = $"{distinctCacheKey}_{distinctPattern}";
        }

        if (memoryCache.TryGetValue(distinctCacheKey, out _)) return true;

        memoryCache.Set(distinctCacheKey, (byte)0x01, rule.Cooldown.Value);
        return false;
    }

    private static async Task NotifyAsync(NotificationRule rule, Dictionary<string, string?>? placeholders, 
        IProvidesPlaceholders?[] placeholderProviders, IServiceScopeFactory serviceScopeFactory, ILogger<NotificationService> logger)
    {
        using var scope = serviceScopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
        if (rule.AlertType == NotificationAlertType.WebHook)
            await NotifyWebHookAsync(rule, placeholders, placeholderProviders, scope, dbContext!, logger);
        else
            throw new NotImplementedException($"Alert type '{rule.AlertType}' not implemented.");
    }

    private static async Task NotifyWebHookAsync(NotificationRule rule, Dictionary<string, string?>? placeholders, 
       IProvidesPlaceholders?[] placeholderProviders, IServiceScope serviceScope, ApplicationDbContext dbContext, ILogger<NotificationService> logger)
    {
        if (string.IsNullOrWhiteSpace(rule.WebHookUrl)) return;

        string? url = rule.WebHookUrl;
        string method = HttpMethod.Get.Method;
        string result = string.Empty;
        try
        {
            var httpClientFactory = serviceScope.ServiceProvider.GetService(typeof(IHttpClientFactory)) as IHttpClientFactory;
            var httpClient = httpClientFactory!.CreateClient();

            var placeholderResolver = serviceScope.ServiceProvider.GetService(typeof(IPlaceholderResolver)) as IPlaceholderResolver;
            url = await placeholderResolver!.ResolvePlaceholdersAsync(url, transformer: HttpUtility.UrlEncode, placeholders: placeholders, placeholderProviders: placeholderProviders);
            var body = await placeholderResolver!.ResolvePlaceholdersAsync(rule.WebHookBody, transformer: null, placeholders: placeholders, placeholderProviders: placeholderProviders);
            var headers = await placeholderResolver!.ResolvePlaceholdersAsync(rule.WebHookHeaders, transformer: null, placeholders: placeholders, placeholderProviders: placeholderProviders);

            var requestData = new CustomRequestData
            {
                RequestMethod = string.IsNullOrWhiteSpace(rule.WebHookMethod) ? HttpMethod.Get.Method : new HttpMethod(rule.WebHookMethod).Method,
                Url = url,
                Body = body,
                Headers = headers
            };

            var httpRequestMessage = requestData?.CreateRequest();
            if (httpRequestMessage == null) return;
            await httpClient.SendAsync(httpRequestMessage);
            result = $"Sent {method} request to '{url}'.";
        }
        catch (Exception ex)
        {
            result = $"Exception while attempting to send {method} request to '{url}': {ex}";
            logger.LogError(ex, "Failed to send webhook notification to '{url}'", url);
        }
        
        var localRule = dbContext.NotificationRules.First(x => x.Id == rule.Id);
        localRule.LastNotifiedAtUtc = DateTime.UtcNow;
        localRule.LastNotifyResult = result;
        await dbContext.SaveChangesAsync();

        dbContext.InvalidateCacheFor<NotificationRule>();
    }
}
