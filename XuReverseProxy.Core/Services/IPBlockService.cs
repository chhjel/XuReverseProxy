﻿using Microsoft.EntityFrameworkCore;
using QoDL.Toolkit.Core.Util;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Services;

public interface IIPBlockService
{
    Task<bool> IsIPBlockedAsync(string ip);
    Task<BlockedIpData?> GetMatchingBlockedIpDataForAsync(string ip);
    Task<BlockedIpData> BlockIPAsync(string ip, string note, Guid? relatedClientId);
    Task<BlockedIpData> BlockIPRegexAsync(string ipRegex, string note, Guid? relatedClientId);
    Task<BlockedIpData> BlockIPCidrRangeAsync(string ipCidr, string note, Guid? relatedClientId);
    Task RemoveIPBlockByIdAsync(Guid id);
}

public class IPBlockService : IIPBlockService
{
    private readonly ApplicationDbContext _dbContext;

    public IPBlockService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> IsIPBlockedAsync(string ip)
        => (await GetMatchingBlockedIpDataForAsync(ip)) != null;

    public async Task<BlockedIpData?> GetMatchingBlockedIpDataForAsync(string ip)
    {
        var ipdatas = await _dbContext.BlockedIpDatas.ToListAsync();
        foreach (var ipdata in ipdatas)
        {
            if (ipdata.IP?.Equals(ip, StringComparison.OrdinalIgnoreCase) == true)
                return await handleMatch(ipdata);
            else if (!string.IsNullOrWhiteSpace(ipdata.IPRegex) && TryRegexMatch(ipdata.IPRegex, ip))
                return await handleMatch(ipdata);
            else if (!string.IsNullOrWhiteSpace(ipdata.CidrRange) && TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(ip, ipdata.CidrRange))
                return await handleMatch(ipdata);
        }

        return null;

        async Task<BlockedIpData> handleMatch(BlockedIpData item)
        {
            await cleanupIfNeeded(item);
            return item;
        }
        
        async Task cleanupIfNeeded(BlockedIpData item)
        {
            if (item.BlockedUntilUtc == null || item.BlockedUntilUtc > DateTime.UtcNow) return;
            _dbContext.Remove(item);
            await _dbContext.SaveChangesAsync();
        }
    }

    private static readonly TKCachedRegexContainer _regexCache = new();
    private static bool TryRegexMatch(string pattern, string value) {
        try
        {
            var regex = _regexCache.GetRegex(pattern, false);
            return regex.IsMatch(value);
        }
        catch (Exception) { return false;  }
    }

    public async Task<BlockedIpData> BlockIPAsync(string ip, string note, Guid? relatedClientId)
    {
        var data = CreateNewIpData(note, relatedClientId);
        data.Type = BlockedIpDataType.IP;
        data.IP = ip;
        _dbContext.Add(data);
        await _dbContext.SaveChangesAsync();
        return data;
    }

    public async Task<BlockedIpData> BlockIPRegexAsync(string ipRegex, string note, Guid? relatedClientId)
    {
        var data = CreateNewIpData(note, relatedClientId);
        data.Type = BlockedIpDataType.IPRegex;
        data.IPRegex = ipRegex;
        _dbContext.Add(data);
        await _dbContext.SaveChangesAsync();
        return data;
    }

    public async Task<BlockedIpData> BlockIPCidrRangeAsync(string ipCidr, string note, Guid? relatedClientId)
    {
        var data = CreateNewIpData(note, relatedClientId);
        data.Type = BlockedIpDataType.CIDRRange;
        data.CidrRange = ipCidr;
        _dbContext.Add(data);
        await _dbContext.SaveChangesAsync();
        return data;
    }

    private static BlockedIpData CreateNewIpData(string? note, Guid? relatedClientId)
        => new()
        {
            BlockedAt = DateTime.UtcNow,
            Note = note,
            RelatedClientId = relatedClientId
        };

    public async Task RemoveIPBlockByIdAsync(Guid id)
    {
        var entity = _dbContext.BlockedIpDatas.FirstOrDefault(x => x.Id == id);
        if (entity == null) return;
        _dbContext.Remove(entity);
        await _dbContext.SaveChangesAsync();
    }
}