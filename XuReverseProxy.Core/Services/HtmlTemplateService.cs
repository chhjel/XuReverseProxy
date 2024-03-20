using Microsoft.EntityFrameworkCore;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Services;

public interface IHtmlTemplateService
{
    Task<HtmlTemplate> GetHtmlTemplateAsync(HtmlTemplateType type, ProxyConfig? proxyConfig = null);
    Task<HtmlTemplate> UpdateHtmlTemplateAsync(HtmlTemplate template);
}

public class HtmlTemplateService(ApplicationDbContext dbContext) : IHtmlTemplateService
{
    public async Task<HtmlTemplate> GetHtmlTemplateAsync(HtmlTemplateType type, ProxyConfig? proxyConfig = null)
    {
        // Not asking for an override = use global
        if (proxyConfig == null) return await GetGlobalHtmlTemplateAsync(type);

        // Locate override if any
        var templateOverride = await GetHtmlTemplateFromDbAsync(type, proxyConfig?.Id);
        
        // Use override with fallback to global
        return templateOverride ?? await GetGlobalHtmlTemplateAsync(type);
    }

    private async Task<HtmlTemplate> GetGlobalHtmlTemplateAsync(HtmlTemplateType type)
    {
        var template = await GetHtmlTemplateFromDbAsync(type, null);
        if (template == null)
        {
            HtmlTemplate.HtmlTemplateDefaults.TryGetValue(type, out template);
            if (template != null)
            {
                template = await UpdateHtmlTemplateAsync(template);
            }
        }

        return template ?? HtmlTemplate.FallbackHtmlTemplate;
    }

    public async Task<HtmlTemplate> UpdateHtmlTemplateAsync(HtmlTemplate template)
    {
        var existing = await dbContext.HtmlTemplates.FirstOrDefaultAsync(x => x.Type == template.Type);
        if (existing == null)
        {
            dbContext.HtmlTemplates.Add(template);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(template);
        }

        await dbContext.SaveChangesAsync();

        dbContext.InvalidateCacheFor<HtmlTemplate>();
        return template;
    }

    private async Task<HtmlTemplate?> GetHtmlTemplateFromDbAsync(HtmlTemplateType type, Guid? proxyConfigId)
    {
        return (await dbContext.GetWithCacheAsync(x => x.HtmlTemplates)).FirstOrDefault(x => x.Type == type && x.ProxyConfigId == proxyConfigId);
    }
}
