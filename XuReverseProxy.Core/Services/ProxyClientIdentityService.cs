using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QoDL.Toolkit.Core.Util;
using QoDL.Toolkit.Web.Core.Utils;
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

    //public async Task<>
    // todo: job that deletes the following:
    // - identities that havent solved all challenges and are older than n days (n = configurable, only if n != null)
    // - identities that have solved all challenges and are older than n days (n = configurable, only if n != null)
}

public class ProxyClientIdentityService : IProxyClientIdentityService
{
    private const string _cookieName = "___xupid";
    private readonly IOptionsMonitor<ServerConfig> _serverConfig;
    private readonly ApplicationDbContext _dbContext;
    private readonly IProxyAuthenticationConditionChecker _proxyAuthenticationConditionChecker;

    public ProxyClientIdentityService(IOptionsMonitor<ServerConfig> serverConfig, ApplicationDbContext dbContext,
        IProxyAuthenticationConditionChecker proxyAuthenticationConditionChecker)
    {
        _serverConfig = serverConfig;
        _dbContext = dbContext;
        _proxyAuthenticationConditionChecker = proxyAuthenticationConditionChecker;
    }

    public async Task<ProxyClientIdentity?> GetProxyClientIdentityAsync(Guid id)
        => await _dbContext.ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == id);

    public async Task<ProxyClientIdentity?> GetCurrentProxyClientIdentityAsync(HttpContext context)
    {
        if (context == null
            || context.Request == null
            || context.Response == null
            || context.Response.Cookies == null) return null;

        if (context.Request.Cookies?.TryGetValue(_cookieName, out string? idFromCookieRaw) != true
            || !Guid.TryParse(idFromCookieRaw, out Guid identityId))
        {
            identityId = Guid.NewGuid();
            context.Response.Cookies.Append(_cookieName, identityId.ToString(),
                new CookieOptions()
                {
                    Domain = $".{_serverConfig.CurrentValue.Domain.Domain}",
                    Expires = DateTimeOffset.Now + (TimeSpan.FromDays(365) * 100),
                    Secure = true,
                    HttpOnly = true,
                    IsEssential = true
                });
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

    public async Task<bool> BlockIdentityAsync(Guid identityId, string message)
    {
        var data = await _dbContext.ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == identityId);
        if (data == null) return false;

        data.Blocked = true;
        data.BlockedMessage = message;
        data.BlockedAtUtc = DateTime.UtcNow;

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
