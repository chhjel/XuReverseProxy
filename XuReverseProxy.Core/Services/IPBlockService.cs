using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QoDL.Toolkit.Core.Util;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Services;

public interface IIPBlockService
{
    Task<bool> IsIPBlockedAsync(string ip);
    Task<BlockedIpData?> GetMatchingBlockedIpDataForAsync(string ip, bool allowDisabled);
    Task<BlockedIpData> BlockIPAsync(string ip, string note, Guid? relatedClientId);
    Task<BlockedIpData> BlockIPRegexAsync(string ipRegex, string note, Guid? relatedClientId);
    Task<BlockedIpData> BlockIPCidrRangeAsync(string ipCidr, string note, Guid? relatedClientId);
    Task RemoveIPBlockByIdAsync(Guid id);
    bool TryRegexMatch(string pattern, string value);
    void InvalidateCache();
}

public class IPBlockService : IIPBlockService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMemoryCache _memoryCache;
    private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public IPBlockService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _memoryCache = memoryCache;
    }

    public async Task<bool> IsIPBlockedAsync(string ip)
        => (await GetMatchingBlockedIpDataForAsync(ip, allowDisabled: false)) != null;

    public async Task<BlockedIpData?> GetMatchingBlockedIpDataForAsync(string ip, bool allowDisabled)
    {
        var ipdatas = await GetIpDatasWithCacheAsync();
        foreach (var ipdata in ipdatas)
        {
            if (!allowDisabled && !ipdata.Enabled) continue;

            if (ipdata.IP?.Equals(ip, StringComparison.OrdinalIgnoreCase) == true)
                return await handleMatch(ipdata);
            else if (!string.IsNullOrWhiteSpace(ipdata.IPRegex) && TryRegexMatch(ipdata.IPRegex, ip))
                return await handleMatch(ipdata);
            else if (!string.IsNullOrWhiteSpace(ipdata.CidrRange) && TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(ip, ipdata.CidrRange))
                return await handleMatch(ipdata);
        }

        return null;

        async Task<BlockedIpData?> handleMatch(BlockedIpData item)
        {
            if (await cleanupIfExpired(item)) return null;
            return item;
        }

        async Task<bool> cleanupIfExpired(BlockedIpData item)
        {
            if (item.BlockedUntilUtc == null || item.BlockedUntilUtc > DateTime.UtcNow) return false;
            item.Enabled = false;
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }

    public async Task<BlockedIpData> BlockIPAsync(string ip, string note, Guid? relatedClientId)
    {
        var data = CreateNewIpData(note, relatedClientId);
        data.Name = $"Block '{ip}'";
        data.Type = BlockedIpDataType.IP;
        data.IP = ip;
        _dbContext.Add(data);

        _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(_httpContextAccessor.HttpContext, $"Blocked IP '{ip}'."));

        await _dbContext.SaveChangesAsync();
        InvalidateCache();
        return data;
    }

    public async Task<BlockedIpData> BlockIPRegexAsync(string ipRegex, string note, Guid? relatedClientId)
    {
        var data = CreateNewIpData(note, relatedClientId);
        data.Name = $"Block regex '{ipRegex}'";
        data.Type = BlockedIpDataType.IPRegex;
        data.IPRegex = ipRegex;
        _dbContext.Add(data);

        _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(_httpContextAccessor.HttpContext, $"Blocked IP by RegEx '{ipRegex}'."));

        await _dbContext.SaveChangesAsync();
        InvalidateCache();
        return data;
    }

    public async Task<BlockedIpData> BlockIPCidrRangeAsync(string ipCidr, string note, Guid? relatedClientId)
    {
        var data = CreateNewIpData(note, relatedClientId);
        data.Name = $"Block CIDR range '{ipCidr}'";
        data.Type = BlockedIpDataType.CIDRRange;
        data.CidrRange = ipCidr;
        _dbContext.Add(data);

        _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(_httpContextAccessor.HttpContext, $"Blocked IP CIDR range '{ipCidr}'."));

        await _dbContext.SaveChangesAsync();
        InvalidateCache();
        return data;
    }

    public async Task RemoveIPBlockByIdAsync(Guid id)
    {
        var entity = _dbContext.BlockedIpDatas.FirstOrDefault(x => x.Id == id);
        if (entity == null) return;
        _dbContext.Remove(entity);

        var val = entity.IP;
        if (string.IsNullOrWhiteSpace(val)) val = entity.IPRegex;
        else if (string.IsNullOrWhiteSpace(val)) val = entity.CidrRange;

        _dbContext.AdminAuditLogEntries.Add(new AdminAuditLogEntry(_httpContextAccessor.HttpContext, $"Removed IP block '{val}'."));

        await _dbContext.SaveChangesAsync();
        InvalidateCache();
    }

    private static readonly TKCachedRegexContainer _regexCache = new();
    public bool TryRegexMatch(string pattern, string value)
    {
        try
        {
            var regex = _regexCache.GetRegex(pattern, false);
            return regex.IsMatch(value);
        }
        catch (Exception) { return false; }
    }

    private static BlockedIpData CreateNewIpData(string? note, Guid? relatedClientId)
        => new()
        {
            Enabled = true,
            BlockedAt = DateTime.UtcNow,
            Note = note,
            RelatedClientId = relatedClientId
        };

    private const string _cacheKey = $"{nameof(IPBlockService)}_allIpRules";
    public void InvalidateCache() => _memoryCache.Remove(_cacheKey);
    private async Task<List<BlockedIpData>> GetIpDatasWithCacheAsync()
    {
        if (_memoryCache.TryGetValue(_cacheKey, out List<BlockedIpData>? val) && val != null) return val;

        var data = await _dbContext.BlockedIpDatas.ToListAsync();
        _memoryCache.Set(_cacheKey, data, _cacheDuration);

        return data;
    }
}
