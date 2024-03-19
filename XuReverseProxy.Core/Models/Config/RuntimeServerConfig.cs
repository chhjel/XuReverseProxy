using Microsoft.AspNetCore.Http;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Web.Core.Utils;
using XuReverseProxy.Core.Logging;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Models.Config;

/// <summary>
/// Dynamic config that can be changed at runtime.
/// </summary>
public class RuntimeServerConfig
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RuntimeServerConfig(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Global killswitch to disable all proxies temporarily.
    /// </summary>
    public bool EnableForwarding
    {
        get => GetConfigBool(nameof(EnableForwarding), defaultValue: true);
        set => SetConfigBool(nameof(EnableForwarding), value);
    }

    /// <summary>
    /// Global killswitch to disable all notifications temporarily.
    /// </summary>
    public bool EnableNotifications
    {
        get => GetConfigBool(nameof(EnableNotifications), defaultValue: true);
        set => SetConfigBool(nameof(EnableNotifications), value);
    }

    // todo: move to server config?
    /// <summary></summary>
    public bool EnableManualApprovalPageAuthentication
    {
        get => GetConfigBool(nameof(EnableManualApprovalPageAuthentication), defaultValue: false);
        set => SetConfigBool(nameof(EnableManualApprovalPageAuthentication), value);
    }

    /// <summary></summary>
    public bool EnableMemoryLogging
    {
        get => MemoryLogger.Enabled;
        set => MemoryLogger.Enabled = value;
    }

    /// <summary>
    /// Since the db is not updated with default values until the configs are first changed,
    /// ensure the db rows exist on startup so that we can read them on the admin config page.
    /// </summary>
    public void EnsureDatabaseRows()
    {
#pragma warning disable CA2245 // Do not assign a property to itself. Reason: setter updates db.
        EnableForwarding = EnableForwarding;
        EnableNotifications = EnableNotifications;
        EnableManualApprovalPageAuthentication = EnableManualApprovalPageAuthentication;
#pragma warning restore CA2245 // Do not assign a property to itself
    }

    public bool GetConfigBool(string key, bool defaultValue)
        => GetConfig(key, defaultValue ? "true" : "false")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;

    private void SetConfigBool(string key, bool value)
        => SetConfig(key, value ? "true" : "false");

    private string? GetConfig(string key, string? fallback = null)
    {
        var item = TKAsyncUtils.RunSync(() => _dbContext.GetWithCacheAsync(x => x.RuntimeServerConfigItems)).FirstOrDefault(x => x.Key == key);
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

        _dbContext.InvalidateCacheFor<RuntimeServerConfigItem>();

        void updateCommon(RuntimeServerConfigItem existing)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            existing.LastUpdatedAtUtc = DateTime.UtcNow;
            existing.LastUpdatedBy = httpContext?.User?.Identity?.Name ?? "unknown";
            existing.LastUpdatedSourceIP = TKRequestUtils.GetIPAddress(httpContext!);
        }
    }
}
