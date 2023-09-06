﻿using Microsoft.AspNetCore.Http;
using QoDL.Toolkit.Web.Core.Utils;
using System.Globalization;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Models.Config;

/// <summary>
/// Dynamic config that can be changed at runtime.
/// </summary>
public class RuntimeServerConfig
{
    /// <summary>
    /// Global killswitch to disable all proxies temporarily.
    /// </summary>
    public bool EnableForwarding
    {
        get => GetConfigBool(nameof(EnableForwarding), defaultValue: true);
        set => SetConfigBool(nameof(EnableForwarding), value);
    }

    /// <summary></summary>
    public bool EnableManualApprovalPageAuthentication
    {
        get => GetConfigBool(nameof(EnableManualApprovalPageAuthentication), defaultValue: false);
        set => SetConfigBool(nameof(EnableManualApprovalPageAuthentication), value);
    }

    /// <summary></summary>
    public string NotFoundHtml
    {
        get => GetConfig(nameof(NotFoundHtml)) ?? "<html><head><title>404 | XuReverseProxy</title></head><body>404 / XuReverseProxy</body></html>";
        set => SetConfig(nameof(NotFoundHtml), value);
    }

    /// <summary></summary>
    public string ClientBlockedHtml
    {
        get => GetConfig(nameof(ClientBlockedHtml)) ?? "<html><head><title>Blocked | XuReverseProxy</title></head><body>{{blocked_message}}</body></html>";
        set => SetConfig(nameof(ClientBlockedHtml), value);
    }

    /// <summary></summary>
    public int ClientBlockedResponseCode
    {
        get => GetConfigInt(nameof(ClientBlockedResponseCode), 401);
        set => SetConfigInt(nameof(ClientBlockedResponseCode), value);
    }

    /// <summary></summary>
    public string IPBlockedHtml
    {
        get => GetConfig(nameof(IPBlockedHtml)) ?? "<html><head><title>Blocked | XuReverseProxy</title></head><body>Nope</body></html>";
        set => SetConfig(nameof(IPBlockedHtml), value);
    }

    /// <summary></summary>
    public int IPBlockedResponseCode
    {
        get => GetConfigInt(nameof(IPBlockedResponseCode), 401);
        set => SetConfigInt(nameof(IPBlockedResponseCode), value);
    }

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RuntimeServerConfig(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Since the db is not updated with default values until the configs are first changed,
    /// ensure the db rows exist on startup so that we can read them on the admin config page.
    /// </summary>
    public void EnsureDatabaseRows()
    {
        EnableForwarding = EnableForwarding;
        EnableManualApprovalPageAuthentication = EnableManualApprovalPageAuthentication;
        NotFoundHtml = NotFoundHtml;
        ClientBlockedHtml = ClientBlockedHtml;
        ClientBlockedResponseCode = ClientBlockedResponseCode;
        IPBlockedHtml = IPBlockedHtml;
        IPBlockedResponseCode = IPBlockedResponseCode;
    }

    private int GetConfigInt(string key, int defaultValue)
    {
        var rawValue = GetConfig(key, defaultValue.ToString());
        if (int.TryParse(rawValue, NumberStyles.Integer, null, out var parsedInt)) return parsedInt;
        else return defaultValue;
    }

    private void SetConfigInt(string key, int value)
        => SetConfig(key, value.ToString());

    public bool GetConfigBool(string key, bool defaultValue)
        => GetConfig(key, defaultValue ? "true" : "false")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

    private void SetConfigBool(string key, bool value)
        => SetConfig(key, value ? "true" : "false");

    private string? GetConfig(string key, string? fallback = null)
    {
        var item = _dbContext.RuntimeServerConfigItems.FirstOrDefault(x => x.Key == key);
        return item == null ? fallback : item.Value;
    }

    private void SetConfig(string key, string? value)
    {
        var existing = _dbContext.RuntimeServerConfigItems.FirstOrDefault(x => x.Key == key);
        if (existing != null)
        {
            existing.Value = value;
            updateCommon(existing);
            _dbContext.SaveChanges();
        }
        else
        {
            var item = new RuntimeServerConfigItem
            {
                Key = key,
                Value = value
            };
            updateCommon(item);
            _dbContext.RuntimeServerConfigItems.Add(item);
            _dbContext.SaveChanges();
        }

        void updateCommon(RuntimeServerConfigItem existing)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            existing.LastUpdatedAtUtc = DateTime.UtcNow;
            existing.LastUpdatedBy = httpContext?.User?.Identity?.Name ?? "unknown";
            existing.LastUpdatedSourceIP = TKRequestUtils.GetIPAddress(httpContext!);
        }
    }
}
