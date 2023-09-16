using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using XuReverseProxy.Core.Abstractions;

namespace XuReverseProxy.Core.Models.DbEntity;

// dotnet ef migrations add InitialMigration --project XuReverseProxy.Core -s XuReverseProxy --verbose
//
// ProxyConfig - service.domain.com => 192.168.2.3:1234
// - List<ProxyAuthenticationData> e.g. [login, otp, manual]
//   * Contains type and json of ProxyChallengeType
//   - List<ProxyAuthenticationCondition> e.g. [daterange, weekday]
// 
// ProxyClientIdentity - client session
// - ProxyClientIdentityData - kvp store
// - ProxyClientIdentitySolvedChallengeData - solved challenges state
// 
public class ApplicationDbContext : IdentityDbContext, IDataProtectionKeyContext
{
    public DbSet<ProxyConfig> ProxyConfigs { get; set; }
    public DbSet<ProxyAuthenticationData> ProxyAuthenticationDatas { get; set; }
    public DbSet<ProxyAuthenticationCondition> ProxyAuthenticationConditions { get; set; }
    public DbSet<ProxyClientIdentity> ProxyClientIdentities { get; set; }
    public DbSet<ProxyClientIdentitySolvedChallengeData> ProxyClientIdentitySolvedChallengeDatas { get; set; }
    public DbSet<ProxyClientIdentityData> ProxyClientIdentityDatas { get; set; }
    public DbSet<RuntimeServerConfigItem> RuntimeServerConfigItems { get; set; }
    public DbSet<ApplicationUserRecoveryCode> RecoveryCodes { get; set; }
    public DbSet<BlockedIpData> BlockedIpDatas { get; set; }
    public DbSet<AdminAuditLogEntry> AdminAuditLogEntries { get; set; }
    public DbSet<ClientAuditLogEntry> ClientAuditLogEntries { get; set; }
    public DbSet<NotificationRule> NotificationRules { get; set; }

    // IDataProtectionKeyContext
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected readonly IConfiguration Configuration;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        : base(options)
    {
        Configuration = configuration;
    }

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

        builder.Entity<ProxyAuthenticationData>()
            .HasMany(e => e.Conditions)
            .WithOne(e => e.AuthenticationData)
            .HasForeignKey(e => e.AuthenticationDataId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Entity<ApplicationUser>()
            .HasMany(e => e.RecoveryCodes)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        builder.Entity<ProxyClientIdentitySolvedChallengeData>().HasIndex(x => x.IdentityId);
        builder.Entity<ProxyClientIdentitySolvedChallengeData>().HasIndex(x => x.SolvedId);
        builder.Entity<ProxyClientIdentitySolvedChallengeData>().HasIndex(x => x.AuthenticationId);
        builder.Entity<ProxyAuthenticationData>().HasIndex(x => x.ProxyConfigId);
        builder.Entity<ProxyAuthenticationCondition>().HasIndex(x => x.AuthenticationDataId);
        builder.Entity<RuntimeServerConfigItem>().HasIndex(x => x.Key);
    }

    public bool IsAttached<T>(T entity) where T : class, IHasId
        => Set<T>().Local.Any(e => e.Id == entity.Id);

    public void EnsureDetached<T>(T entity) where T : class, IHasId
    {
        var match = Set<T>().Local.FirstOrDefault(e => e.Id == entity.Id);
        if (match == null) return;
        Entry(match).State = EntityState.Detached;
    }
}
