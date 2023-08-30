using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Models.Common;

namespace XuReverseProxy.Controllers.API;

[Authorize]
public class ProxyConfigController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public ProxyConfigController(ApplicationDbContext context)
    {
        _dbContext = context;
    }

    /// <summary>
    /// Get all configs.
    /// </summary>
    [HttpGet("/api/proxyconfig")]
    public async Task<GenericResultData<List<ProxyConfig>>> GetProxyConfigsAsync()
    {
        try
        {
            var configs = await _dbContext.ProxyConfigs.ToListAsync();
            return GenericResult.CreateSuccess(configs);
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError<List<ProxyConfig>>(ex.Message);
        }
    }

    /// <summary>
    /// Get single config.
    /// </summary>
    [HttpGet("/api/proxyconfig/{configId}")]
    public async Task<GenericResultData<ProxyConfig>> GetProxyConfigAsync([FromRoute] Guid configId)
    {
        try
        {
            var config = await _dbContext.ProxyConfigs.FirstOrDefaultAsync(x => x.Id == configId);
            return (config != null)
                ? GenericResult.CreateSuccess(config)
                : GenericResult.CreateError<ProxyConfig>("Config not found.");
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError<ProxyConfig>(ex.Message);
        }
    }

    /// <summary>
    /// Create or update config.
    /// </summary>
    [HttpPost("/api/proxyconfig")]
    public async Task<GenericResultData<ProxyConfig>> CreateOrUpdateProxyConfigAsync([FromBody] ProxyConfig config)
    {
        try
        {
            // todo
            return GenericResult.CreateSuccess(config);
        }
        catch (Exception ex)
        {
            return GenericResult.CreateError<ProxyConfig>(ex.Message);
        }
    }
}
