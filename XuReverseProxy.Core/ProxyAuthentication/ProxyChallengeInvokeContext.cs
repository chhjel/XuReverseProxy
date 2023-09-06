using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Services;

namespace XuReverseProxy.Core.ProxyAuthentication;

public class ProxyChallengeInvokeContext
{
    public HttpContext HttpContext { get; }
    public ProxyAuthenticationData AuthenticationData { get; }
    public ProxyConfig ProxyConfig { get; }
    public ProxyClientIdentity ClientIdentity { get; }
    public Guid SolvedId => AuthenticationData.SolvedId;

    private readonly IProxyChallengeService _proxyChallengeService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _dbContext;

    public ProxyChallengeInvokeContext(HttpContext httpContext, ProxyAuthenticationData authenticationData,
        ProxyConfig proxyConfig, ProxyClientIdentity clientIdentity,
        ApplicationDbContext dbContext, IServiceProvider serviceProvider, IProxyChallengeService proxyChallengeService)
    {
        HttpContext = httpContext;
        AuthenticationData = authenticationData;
        ProxyConfig = proxyConfig;
        ClientIdentity = clientIdentity;
        _dbContext = dbContext;
        _serviceProvider = serviceProvider;
        _proxyChallengeService = proxyChallengeService;
    }

    public async Task<bool> SetChallengeSolvedAsync()
        => await _proxyChallengeService.SetChallengeSolvedAsync(ClientIdentity.Id, AuthenticationData.Id, SolvedId);

    public T GetService<T>() where T : class
        => (_serviceProvider.GetService(typeof(T)) as T)!;

    /// <summary>
    /// Fetch any stored data related to this challenge.
    /// </summary>
    public async Task<string?> GetDataAsync(string key)
    {
        var data = await _dbContext.ProxyClientIdentityDatas
            .FirstOrDefaultAsync(x => 
                x.IdentityId == ClientIdentity.Id 
                && x.AuthenticationId == AuthenticationData.Id
                && x.Key == key);
        return data?.Value;
    }

    /// <summary>
    /// Store any data related to this challenge.
    /// </summary>
    public async Task SetDataAsync(string key, string value, bool save = true)
    {
        var data = await _dbContext.ProxyClientIdentityDatas.FirstOrDefaultAsync(x =>
                x.IdentityId == ClientIdentity.Id
                && x.AuthenticationId == AuthenticationData.Id
                && x.Key == key);
        if (data == null)
        {
            await _dbContext.ProxyClientIdentityDatas.AddAsync(new ProxyClientIdentityData
            {
                IdentityId = ClientIdentity.Id,
                AuthenticationId = AuthenticationData.Id,
                Key = key,
                Value = value
            });
            
            if (save) await _dbContext.SaveChangesAsync();
        }
        else if (data.Value != value)
        {
            data.Value = value;
            _dbContext.ProxyClientIdentityDatas.Update(data);

            if (save) await _dbContext.SaveChangesAsync();
        }
    }
}
