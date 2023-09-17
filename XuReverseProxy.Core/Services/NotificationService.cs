﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Web;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.Services;

public interface INotificationService
{
    Task TryNotifyEvent(NotificationTrigger trigger, params IProvidesPlaceholders?[] placeholderProviders);
    Task TryNotifyEvent(NotificationTrigger trigger, Dictionary<string, string?>? placeholders, params IProvidesPlaceholders?[] placeholderProviders);
}

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;

    public NotificationService(ApplicationDbContext dbContext, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
    }

    public async Task TryNotifyEvent(NotificationTrigger trigger, params IProvidesPlaceholders?[] placeholderProviders)
        => await TryNotifyEvent(trigger, null, placeholderProviders);

    public async Task TryNotifyEvent(NotificationTrigger trigger, Dictionary<string, string?>? placeholders, params IProvidesPlaceholders?[] placeholderProviders)
    {
        var now = DateTime.UtcNow;
        var matchingRules = await _dbContext.NotificationRules.Where(x => x.Enabled && x.TriggerType == trigger)
            .ToListAsync();

        foreach (var rule in matchingRules)
        {
            if (HandleRuleCooldown(rule, placeholders, placeholderProviders)) continue;

            await NotifyAsync(rule, placeholders, placeholderProviders);
        }
    }

    /// <summary>
    /// Returns true if the rule is on cooldown. Sets cooldown if enabled.
    /// </summary>
    private bool HandleRuleCooldown(NotificationRule rule, Dictionary<string, string?>? placeholders, IProvidesPlaceholders?[] placeholderProviders)
    {
        if (!(rule.Cooldown > TimeSpan.Zero)) return false;

        var distinctCacheKey = $"_notify_distinction_{rule.Id}";
        if (!string.IsNullOrWhiteSpace(rule.CooldownDistinctPattern))
        {
            var distinctPattern = rule.CooldownDistinctPattern;
            if (placeholders != null) distinctPattern = PlaceholderUtils.ResolvePlaceholders(distinctPattern, placeholders);
            distinctPattern = PlaceholderUtils.ResolvePlaceholders(distinctPattern, placeholderProviders);
            distinctCacheKey = $"{distinctCacheKey}_{distinctPattern}";
        }

        if (_memoryCache.TryGetValue(distinctCacheKey, out _)) return true;

        _memoryCache.Set(distinctCacheKey, (byte)0x01, rule.Cooldown.Value);
        return false;
    }

    private async Task NotifyAsync(NotificationRule rule, Dictionary<string, string?>? placeholders, IProvidesPlaceholders?[] placeholderProviders)
    {
        if (rule.AlertType == NotificationAlertType.WebHook)
            await NotifyWebHookAsync(rule, placeholders, placeholderProviders);
        else
            throw new NotImplementedException($"Alert type '{rule.AlertType}' not implemented.");
    }

    private async Task NotifyWebHookAsync(NotificationRule rule, Dictionary<string, string?>? placeholders, IProvidesPlaceholders?[] placeholderProviders)
    {
        if (string.IsNullOrWhiteSpace(rule.WebHookUrl)) return;

        var method = HttpMethod.Get;
        string? url = rule.WebHookUrl;
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            if (placeholders?.Any() == true)
            {
                url = PlaceholderUtils.ResolvePlaceholders(url, transformer: HttpUtility.UrlEncode, placeholders);
            }
            url = PlaceholderUtils.ResolvePlaceholders(url, transformer: HttpUtility.UrlEncode, placeholderProviders);

            if (!string.IsNullOrWhiteSpace(rule.WebHookMethod)) method = new HttpMethod(rule.WebHookMethod);

            var httpRequestMessage = new HttpRequestMessage(method, url);
            // todo: body
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
