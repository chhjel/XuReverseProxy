﻿using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Web.Core.Utils;
using XuReverseProxy.Core.Extensions;
using XuReverseProxy.Core.Logging;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Services;

public interface IProxyClientIdentityService
{
    Task<ProxyClientIdentity?> GetProxyClientIdentityAsync(Guid id);
    Task<ProxyClientIdentity?> GetCurrentProxyClientIdentityAsync(HttpContext context);

    Task TryUpdateLastAccessedAtAsync(Guid id);
    Task<bool> BlockIdentityAsync(Guid identityId, string message);
    Task<bool> UnBlockIdentityAsync(Guid identityId);
    Task<bool> SetClientNoteAsync(Guid identityId, string note);
}

public class ProxyClientIdentityService : IProxyClientIdentityService
{
    public const string ClientIdCookieName = "___xupid";
    private readonly IOptionsMonitor<ServerConfig> _serverConfig;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ProxyClientIdentityService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IDataProtector _dataProtector;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly INotificationService _notificationService;

    public ProxyClientIdentityService(IOptionsMonitor<ServerConfig> serverConfig, ApplicationDbContext dbContext, ILogger<ProxyClientIdentityService> logger,
        IMemoryCache memoryCache, IDataProtectionProvider dataProtectorProvider, IHttpContextAccessor httpContextAccessor, INotificationService notificationService)
    {
        _serverConfig = serverConfig;
        _dbContext = dbContext;
        _logger = logger;
        _memoryCache = memoryCache;
        _dataProtector = dataProtectorProvider.CreateProtector("XuReverseProxy");
        _httpContextAccessor = httpContextAccessor;
        _notificationService = notificationService;
    }

    public async Task<ProxyClientIdentity?> GetProxyClientIdentityAsync(Guid id)
        => await _dbContext.ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<ProxyClientIdentity?> GetCurrentProxyClientIdentityAsync(HttpContext context)
    {
        if (context == null
            || context.Request == null
            || context.Response == null
            || context.Response.Cookies == null) return null;

        // Create cookie if not set
        var isNewIdentity = false;
        if (context.Request.Cookies?.TryGetValue(ClientIdCookieName, out string? valueFromCookie) != true
            || !TryUnprotect(valueFromCookie, out var clientIdRaw)
            || !Guid.TryParse(clientIdRaw, out Guid identityId))
        {
            if (!AllowCreatingNewClientFrom(context)) return null;

            identityId = Guid.NewGuid();
            isNewIdentity = true;
            context.Response.Cookies.Append(ClientIdCookieName, _dataProtector.Protect(identityId.ToString()), CreateClientCookieOptions());

            if (MemoryLogger.Enabled) _logger.LogInformation("Creating new client identity '{identityId}' from {Method} request to '{path}'", identityId, context.Request.Method, context.Request.Path);
        }

        // Extend client cookie periodically
        var cookieExtendCacheKey = $"_cext_{identityId}";
        if (!isNewIdentity && !_memoryCache.TryGetValue<byte>(cookieExtendCacheKey, out _))
        {
            context.Response.Cookies.Append(ClientIdCookieName, _dataProtector.Protect(identityId.ToString()), CreateClientCookieOptions());
            _memoryCache.Set(cookieExtendCacheKey, (byte)0x01, TimeSpan.FromMinutes(5));
        }

        var rawIp = TKRequestUtils.GetIPAddress(context);
        var ipData = TKIPAddressUtils.ParseIP(rawIp, acceptLocalhostString: true);
        var ip = ipData.IP;
        var userAgent = context.Request.Headers.UserAgent; // CleanUserAgent(context.Request.Headers.UserAgent);

        // Get or create identity
        var identity = await _dbContext.ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == identityId);
        if (identity == null)
        {
            identity = new ProxyClientIdentity
            {
                Id = identityId,
                IP = ip,
                UserAgent = userAgent,
                CreatedAtUtc = DateTime.UtcNow,
                LastAttemptedAccessedAtUtc = DateTime.UtcNow
            };
            await _dbContext.ProxyClientIdentities.AddAsync(identity);
            await _dbContext.SaveChangesAsync();
            await _notificationService.TryNotifyEvent(NotificationTrigger.NewClient,
                new Dictionary<string, string?> {
                    { "Url", context.Request.GetDisplayUrl() }
                }, identity);
            return identity;
        }

        // Check for any changes to update
        var shouldUpdate = false;
        if (identity.IP != ip)
        {
            identity.IP = ip;
            shouldUpdate = true;
        }
        if (identity.UserAgent != userAgent)
        {
            identity.UserAgent = userAgent;
            shouldUpdate = true;
        }
        if ((DateTime.UtcNow - identity.LastAttemptedAccessedAtUtc) > TimeSpan.FromMinutes(5))
        {
            shouldUpdate = true;
        }
        if (shouldUpdate)
        {
            identity.LastAttemptedAccessedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        return identity;
    }

    // Browsers send a request to some of these without cookies, causing lots of extra clients created.
    // todo: find a better way to fix this.
    private static readonly HashSet<string> _pathsNotAllowedIdentityCreation = new(new[]
    {
        "/favicon.ico",
        "/apple-touch-icon.png",
        "/robots.txt",
        "/sitemap.xml",
        "/humans.txt",
        "/crossdomain.xml",
        "/browserconfig.xml",
        "/manifest.json",
        "/ads.txt"
    });
    private bool AllowCreatingNewClientFrom(HttpContext context)
        => context.Request.Method == HttpMethod.Get.Method
        && !_pathsNotAllowedIdentityCreation.Contains(context.Request.Path)
        && !context.Request.Path.ToString().StartsWith("/.well-known/")
        && !context.Request.Path.ToString().StartsWith("/applet/");

    private bool TryUnprotect(string? protectedValue, out string unprotectedValue)
    {
        unprotectedValue = string.Empty;
        if (protectedValue == null) return false;

        try
        {
            unprotectedValue = _dataProtector.Unprotect(protectedValue);
            return true;
        } catch(Exception) {
            return false;
        }
    }

    private CookieOptions CreateClientCookieOptions()
    {
        var serverConfig = _serverConfig.CurrentValue;
        return new()
        {
            Domain = $".{serverConfig.Domain.Domain}",
            Expires = DateTimeOffset.Now + TimeSpan.FromMinutes(serverConfig.Security.ClientCookieLifetimeInMinutes),
            Secure = true,
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        };
    }

    public async Task<bool> BlockIdentityAsync(Guid identityId, string message)
    {
        var data = await _dbContext.ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == identityId);
        if (data == null) return false;

        data.Blocked = true;
        data.BlockedMessage = message;
        data.BlockedAtUtc = DateTime.UtcNow;

        _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(_httpContextAccessor.HttpContext,
                $"Blocked client {AdminAuditLogEntry.Placeholder_Client}")
                .SetRelatedClient(data.Id, data.NameForLog())
        );
        _dbContext.ClientAuditLogEntries.Add(new ClientAuditLogEntry(_httpContextAccessor.HttpContext, identityId, data.NameForLog(), $"Was blocked"));
        await _dbContext.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UnBlockIdentityAsync(Guid identityId)
    {
        var data = await _dbContext.ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == identityId);
        if (data == null) return false;

        data.Blocked = false;
        data.BlockedMessage = null;
        data.BlockedAtUtc = null;

        _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(_httpContextAccessor.HttpContext,
                $"Unblocked client {AdminAuditLogEntry.Placeholder_Client}")
                .SetRelatedClient(data.Id, data.NameForLog())
            );
        _dbContext.ClientAuditLogEntries.Add(new ClientAuditLogEntry(_httpContextAccessor.HttpContext, identityId, data.NameForLog(), $"Was unblocked"));

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetClientNoteAsync(Guid identityId, string note)
    {
        var data = await _dbContext.ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == identityId);
        if (data == null) return false;

        data.Note = note;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task TryUpdateLastAccessedAtAsync(Guid identityId)
    {
        var data = await _dbContext.ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == identityId);
        if (data == null) return;

        // Only update last accessed timestamp after 5 minutes
        if ((DateTime.UtcNow - data.LastAccessedAtUtc) < TimeSpan.FromMinutes(5)) return;

        data.LastAccessedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
}
