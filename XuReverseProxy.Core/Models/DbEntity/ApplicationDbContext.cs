using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace XuReverseProxy.Core.Models.DbEntity;

// dotnet ef migrations add <migration_name> --project XuReverseProxy.Core -s XuReverseProxy --verbose
// dotnet ef migrations remove --project XuReverseProxy.Core -s XuReverseProxy --verbose

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration, IMemoryCache memoryCache) : IdentityDbContext(options), IDataProtectionKeyContext
{
    public DbSet<ProxyConfig> ProxyConfigs { get; set; }
    public DbSet<ConditionData> ConditionDatas { get; set; }
    public DbSet<ProxyAuthenticationData> ProxyAuthenticationDatas { get; set; }
    public DbSet<ProxyClientIdentity> ProxyClientIdentities { get; set; }
    public DbSet<ProxyClientIdentitySolvedChallengeData> ProxyClientIdentitySolvedChallengeDatas { get; set; }
    public DbSet<ProxyClientIdentityData> ProxyClientIdentityDatas { get; set; }
    public DbSet<RuntimeServerConfigItem> RuntimeServerConfigItems { get; set; }
    public DbSet<GlobalVariable> GlobalVariables { get; set; }
    public DbSet<ApplicationUserRecoveryCode> RecoveryCodes { get; set; }
    public DbSet<BlockedIpData> BlockedIpDatas { get; set; }
    public DbSet<AdminAuditLogEntry> AdminAuditLogEntries { get; set; }
    public DbSet<ClientAuditLogEntry> ClientAuditLogEntries { get; set; }
    public DbSet<NotificationRule> NotificationRules { get; set; }
    public DbSet<ConditionData> Conditions { get; set; }
    public DbSet<HtmlTemplate> HtmlTemplates { get; set; }

    // IDataProtectionKeyContext
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected readonly IConfiguration Configuration = configuration;
    private static readonly MemoryCache _clientMemoryCache = new(new MemoryCacheOptions() { SizeLimit = 10000 });

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Configuration.GetConnectionString("DbConnection"));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ProxyClientIdentity>()
            .HasMany(e => e.Data)
            .WithOne(e => e.Identity)
            .HasForeignKey(e => e.IdentityId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Entity<ProxyClientIdentity>()
            .HasMany(e => e.SolvedChallenges)
            .WithOne(e => e.Identity)
            .HasForeignKey(e => e.IdentityId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Entity<ProxyConfig>()
            .HasMany(e => e.Authentications)
            .WithOne(e => e.ProxyConfig)
            .HasForeignKey(e => e.ProxyConfigId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Entity<ApplicationUser>()
            .HasMany(e => e.RecoveryCodes)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        // Conditions
        builder.Entity<ProxyConfig>()
            .HasMany(e => e.ProxyConditions)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProxyAuthenticationData>()
            .HasMany(e => e.Conditions)
            .WithOne()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ProxyClientIdentitySolvedChallengeData>().HasIndex(x => x.IdentityId);
        builder.Entity<ProxyClientIdentitySolvedChallengeData>().HasIndex(x => x.SolvedId);
        builder.Entity<ProxyClientIdentitySolvedChallengeData>().HasIndex(x => x.AuthenticationId);
        builder.Entity<ProxyAuthenticationData>().HasIndex(x => x.ProxyConfigId);
        builder.Entity<RuntimeServerConfigItem>().HasIndex(x => x.Key);
        builder.Entity<GlobalVariable>().HasIndex(x => x.Name);
        builder.Entity<HtmlTemplate>().HasIndex(x => x.Type);
    }

    public void InvalidateCacheFor<T>()
    {
        memoryCache.Remove($"all_{typeof(T)}");

        if (typeof(T) == typeof(ProxyAuthenticationData))
        {
            memoryCache.Remove($"all_{typeof(ProxyConfig)}");
            memoryCache.Remove($"all_{typeof(ConditionData)}");
        }
        else if (typeof(T) == typeof(ConditionData))
        {
            memoryCache.Remove($"all_{typeof(ProxyConfig)}");
            memoryCache.Remove($"all_{typeof(ProxyAuthenticationData)}");
        }
        else if (typeof(T) == typeof(ProxyConfig))
        {
            memoryCache.Remove($"all_{typeof(ProxyAuthenticationData)}");
            memoryCache.Remove($"all_{typeof(ConditionData)}");
        }
    }

    public async Task<List<T>> GetWithCacheAsync<T>(Func<ApplicationDbContext, DbSet<T>> entities)
        where T : class
    {
        var cacheKey = $"all_{typeof(T)}";
        if (memoryCache.TryGetValue(cacheKey, out var val) && val is List<T> list) return list;

        var data = entities(this);

        list = await LoadEntitiesForCacheAsync(data);
        memoryCache.Set(cacheKey, list, TimeSpan.FromMinutes(5));
        return list;
    }

    private async Task<List<T>> LoadEntitiesForCacheAsync<T>(DbSet<T> dbSet)
        where T : class
    {
        if (dbSet is DbSet<ProxyConfig> proxyConfigSet)
        {
            return (await proxyConfigSet
                .Include(x => x.ProxyConditions)
                .Include(x => x.Authentications)
                .ThenInclude(x => x.Conditions)
                .ToArrayAsync())
                .Cast<T>()
                .ToList();
        }
        else
        {
            return await dbSet.ToListAsync();
        }
    }

    public void InvalidateClientCache(Guid id)
        => _clientMemoryCache.Remove($"{id}");

    public async Task<ProxyClientIdentity?> GetClientWithCacheAsync(Guid id)
    {
        var cacheKey = $"{id}";
        if (_clientMemoryCache.TryGetValue(cacheKey, out var val) && val is ProxyClientIdentity item) return item;

        var client = await ProxyClientIdentities.FirstOrDefaultAsync(x => x.Id == id);
        if (client != null) _clientMemoryCache.Set(cacheKey, client, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            Size = 1
        });
        return client;
    }
}
