using System.Globalization;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Services;

public interface IPlaceholderResolver
{
    Task<string?> ResolvePlaceholdersAsync(string? template, Func<string?, string?>? transformer = null,
        Dictionary<string, string?>? placeholders = null, params IProvidesPlaceholders?[] placeholderProviders);
}

public class PlaceholderResolver : IPlaceholderResolver
{
    private readonly ApplicationDbContext _dbContext;
    private static readonly CultureInfo _placeholderLocale = CultureInfo.CreateSpecificCulture("en");

    public PlaceholderResolver(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> ResolvePlaceholdersAsync(string? template,
        Func<string?, string?>? transformer = null,
        Dictionary<string, string?>? placeholders = null,
        params IProvidesPlaceholders?[] placeholderProviders)
    {
        template = await ResolvePlaceholdersInternalAsync(template, transformer, placeholders, placeholderProviders, iterationCounter: 0);
        return template;
    }

    private async Task<string?> ResolvePlaceholdersInternalAsync(string? template,
        Func<string?, string?>? transformer,
        Dictionary<string, string?>? placeholders,
        IProvidesPlaceholders?[] placeholderProviders,
        int iterationCounter)
    {
        if (string.IsNullOrWhiteSpace(template)) return template;
        var originalTemplate = template;

        transformer ??= v => v ?? string.Empty;

        // Given dict
        template = ResolveDictionaryPlaceholders(template, transformer, placeholders);

        // Global variables
        template = await ResolveGlobalVariablesAsync(template, transformer)!;

        // Common
        template = ResolveCommonPlaceholders(template, transformer)!;

        // Given providers
        foreach (var placeholderProvider in placeholderProviders!)
        {
            if (placeholderProvider != null) template = placeholderProvider.ResolvePlaceholders(template, transformer);
        }

        if (iterationCounter < 3 && template != originalTemplate)
        {
            template = await ResolvePlaceholdersInternalAsync(template, transformer, placeholders, placeholderProviders, iterationCounter++);
        }

        return template;
    }

    private string? ResolveDictionaryPlaceholders(string? template, Func<string?, string?>? transformer, Dictionary<string, string?>? placeholders)
    {
        if (placeholders == null || string.IsNullOrWhiteSpace(template)) return template;
        string transform(string val) => transformer?.Invoke(val) ?? val;

        foreach (var placeholder in placeholders)
        {
            var value = placeholder.Value == null ? string.Empty : transform(placeholder.Value);
            template = template?.Replace($"{{{{{placeholder.Key}}}}}", value, StringComparison.OrdinalIgnoreCase);
        }

        return template;
    }

    private string? ResolveCommonPlaceholders(string? template, Func<string?, string?>? transformer = null)
    {
        if (string.IsNullOrWhiteSpace(template)) return template;
        string transform(string val) => transformer?.Invoke(val) ?? val;

        return template
            ?.Replace("{{weekday}}", transform(DateTime.Now.DayOfWeek.ToString()), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{monthName}}", transform(DateTime.Now.ToString("MMMM", _placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{dayOfMonth}}", transform(DateTime.Now.Day.ToString(_placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{hour}}", transform(DateTime.Now.Hour.ToString(_placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{minute}}", transform(DateTime.Now.Minute.ToString(_placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{month}}", transform(DateTime.Now.Month.ToString(_placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{year}}", transform(DateTime.Now.Year.ToString(_placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{guid}}", transform(Guid.NewGuid().ToString()), StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string?> ResolveGlobalVariablesAsync(string? template, Func<string?, string?>? transformer)
    {
        if (string.IsNullOrWhiteSpace(template)) return template;
        string transform(string val) => transformer?.Invoke(val) ?? val;

        var globalVars = await _dbContext.GetWithCacheAsync(x => x.GlobalVariables);
        foreach (var gvar in globalVars)
        {
            if (string.IsNullOrWhiteSpace(gvar.Name)) continue;
            template = template?.Replace($"{{{{{gvar.Name}}}}}", gvar.Value == null ? string.Empty : transform(gvar.Value), StringComparison.OrdinalIgnoreCase);
        }

        return template;
    }
}
