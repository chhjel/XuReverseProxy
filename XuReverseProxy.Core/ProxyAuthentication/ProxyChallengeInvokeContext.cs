using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Core.Services;

namespace XuReverseProxy.Core.ProxyAuthentication;

public class ProxyChallengeInvokeContext(HttpContext httpContext, ProxyAuthenticationData authenticationData,
    ProxyConfig proxyConfig, ProxyClientIdentity clientIdentity,
    ApplicationDbContext dbContext, IServiceProvider serviceProvider, IProxyChallengeService proxyChallengeService)
{
    public HttpContext HttpContext { get; } = httpContext;
    public ProxyAuthenticationData AuthenticationData { get; } = authenticationData;
    public ProxyConfig ProxyConfig { get; } = proxyConfig;
    public ProxyClientIdentity ClientIdentity { get; } = clientIdentity;
    public Guid SolvedId => AuthenticationData.SolvedId;

    public async Task<bool> SetChallengeSolvedAsync()
        => await proxyChallengeService.SetChallengeSolvedAsync(ClientIdentity.Id, AuthenticationData.Id, SolvedId);

    public T GetService<T>() where T : class
        => (serviceProvider.GetService(typeof(T)) as T)!;

    /// <summary>
    /// Fetch any stored data related to this challenge.
    /// </summary>
    public async Task<string?> GetDataAsync(string key)
    {
        var data = await dbContext.ProxyClientIdentityDatas
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
        var data = await dbContext.ProxyClientIdentityDatas.FirstOrDefaultAsync(x =>
                x.IdentityId == ClientIdentity.Id
                && x.AuthenticationId == AuthenticationData.Id
                && x.Key == key);
        if (data == null)
        {
            await dbContext.ProxyClientIdentityDatas.AddAsync(new ProxyClientIdentityData
            {
                IdentityId = ClientIdentity.Id,
                AuthenticationId = AuthenticationData.Id,
                Key = key,
                Value = value
            });
            
            if (save) await dbContext.SaveChangesAsync();
        }
        else if (data.Value != value)
        {
            data.Value = value;
            dbContext.ProxyClientIdentityDatas.Update(data);

            if (save) await dbContext.SaveChangesAsync();
        }
    }
}
