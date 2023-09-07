using System.Text.Json;
using XuReverseProxy.Core.Extensions;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.ProxyAuthentication.Challenges;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.Services;

// Todo: move to own project & container
public interface IDevDataSeeder
{
    public Task EnsureDemoDataAsync();
}

public class DevDataSeederService : IDevDataSeeder
{
    private readonly ApplicationDbContext _dbContext;

    public DevDataSeederService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task EnsureDemoDataAsync()
    {
        if (_dbContext.ProxyConfigs.Any()) return;

        var configX = new ProxyConfig
        {
            Enabled = true,
            Name = "Test1 proxy",
            Subdomain = "test1",
            Port = null,
            DestinationPrefix = "https://www.mozilla.org"
        };
        await _dbContext.ProxyConfigs.AddAsync(configX);

        var config = new ProxyConfig
        {
            Enabled = true,
            Name = "Test2 proxy",
            Subdomain = "test2",
            Port = null,
            DestinationPrefix = "https://duckduckgo.com",
            ChallengeTitle = "Home Camera Server",
            ChallengeDescription = "To access this resource the following challenges must be completed.",
            ShowChallengesWithUnmetRequirements = true,
            ShowCompletedChallenges = true,
        };
        await _dbContext.ProxyConfigs.AddAsync(config);

        var loginAuth = new ProxyAuthenticationData
        {
            SolvedId = Guid.NewGuid(),
            ChallengeTypeId = nameof(ProxyChallengeTypeLogin),
            //SolvedDuration = TimeSpan.FromMinutes(5),
            ChallengeJson = JsonSerializer.Serialize(new ProxyChallengeTypeLogin
            {
                Username = "test",
                Password = "test"
            }, JsonConfig.DefaultOptions)
        };

        var adminLoginAuth = new ProxyAuthenticationData
        {
            SolvedId = Guid.NewGuid(),
            ChallengeTypeId = nameof(ProxyChallengeTypeAdminLogin),
            //SolvedDuration = TimeSpan.FromMinutes(5),
            ChallengeJson = JsonSerializer.Serialize(new ProxyChallengeTypeAdminLogin
            {
                Description = "Admin login used here."
            }, JsonConfig.DefaultOptions)
        };

        var loginAuthWithConditions = new ProxyAuthenticationData
        {
            SolvedId = Guid.NewGuid(),
            ChallengeTypeId = nameof(ProxyChallengeTypeLogin),
            //SolvedDuration = TimeSpan.FromMinutes(5),
            ChallengeJson = JsonSerializer.Serialize(new ProxyChallengeTypeLogin
            {
                Username = "test",
                Password = "test"
            }, JsonConfig.DefaultOptions)
        };
        loginAuthWithConditions.Conditions.Add(new ProxyAuthenticationCondition()
        {
            ConditionType = ProxyAuthenticationCondition.ProxyAuthenticationConditionType.TimeRange,
            TimeOnlyUtc1 = new TimeOnly(6, 30),
            TimeOnlyUtc2 = new TimeOnly(23, 45),
        });
        loginAuthWithConditions.Conditions.Add(new ProxyAuthenticationCondition()
        {
            ConditionType = ProxyAuthenticationCondition.ProxyAuthenticationConditionType.DateTimeRange,
            DateTimeUtc1 = new DateTime(2022, 2, 12).SetKindUtc(),
            DateTimeUtc2 = new DateTime(2025, 3, 13).SetKindUtc(),
        });
        loginAuthWithConditions.Conditions.Add(new ProxyAuthenticationCondition()
        {
            ConditionType = ProxyAuthenticationCondition.ProxyAuthenticationConditionType.WeekDays,
            DaysOfWeekUtc = ProxyAuthenticationCondition.DayOfWeekFlags.Wednesday
        });
        var otpAuth = new ProxyAuthenticationData
        {
            SolvedId = Guid.NewGuid(),
            ChallengeTypeId = nameof(ProxyChallengeTypeOTP),
            //SolvedDuration = TimeSpan.FromMinutes(5),
            ChallengeJson = JsonSerializer.Serialize(new ProxyChallengeTypeOTP
            {
                WebHookUrl = null,
                WebHookRequestMethod = "GET"
            }, JsonConfig.DefaultOptions)
        };
        var manualAuth = new ProxyAuthenticationData
        {
            SolvedId = Guid.NewGuid(),
            ChallengeTypeId = nameof(ProxyChallengeTypeManualApproval),
            //SolvedDuration = TimeSpan.FromMinutes(5),
            ChallengeJson = JsonSerializer.Serialize(new ProxyChallengeTypeManualApproval
            {
                WebHookUrl = null,
                WebHookRequestMethod = "GET"
            }, JsonConfig.DefaultOptions)
        };
        var secretAuth1 = new ProxyAuthenticationData
        {
            SolvedId = Guid.NewGuid(),
            ChallengeTypeId = nameof(ProxyChallengeTypeSecretQueryString),
            //SolvedDuration = TimeSpan.FromMinutes(5),
            ChallengeJson = JsonSerializer.Serialize(new ProxyChallengeTypeSecretQueryString
            {
                Secret = "test"
            }, JsonConfig.DefaultOptions)
        };
        var secretAuth2 = new ProxyAuthenticationData
        {
            SolvedId = Guid.NewGuid(),
            ChallengeTypeId = nameof(ProxyChallengeTypeSecretQueryString),
            //SolvedDuration = TimeSpan.FromMinutes(5),
            ChallengeJson = JsonSerializer.Serialize(new ProxyChallengeTypeSecretQueryString
            {
                Secret = "test2"
            }, JsonConfig.DefaultOptions)
        };
        var auths = new List<ProxyAuthenticationData>
        {
            secretAuth1,
            secretAuth2,
            loginAuthWithConditions,
            loginAuth,
            adminLoginAuth,
            otpAuth,
            manualAuth
        };
        for (int i=0;i<auths.Count;i++)
        {
            auths[i].Order = i;
            config.Authentications.Add(auths[i]);
        }

        await _dbContext.SaveChangesAsync();
    }
}
