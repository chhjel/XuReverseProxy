using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Core.Services;

public interface IPlaceholderResolver
{
    Task<string?> ResolvePlaceholdersAsync(string? template, Func<string?, string?>? defaultTransformer = null,
        Dictionary<string, string?>? placeholders = null, params IProvidesPlaceholders?[] placeholderProviders);
}

public partial class PlaceholderResolver(ApplicationDbContext dbContext) : IPlaceholderResolver
{
    private static readonly CultureInfo _placeholderLocale = CultureInfo.CreateSpecificCulture("en");
    private static readonly Regex _placeholderRegex = PlaceholderRegex();

    public async Task<string?> ResolvePlaceholdersAsync(string? template,
        Func<string?, string?>? defaultTransformer = null,
        Dictionary<string, string?>? placeholders = null,
        params IProvidesPlaceholders?[] placeholderProviders)
    {
        template = await ResolvePlaceholdersInternalAsync(template, defaultTransformer, placeholders,
            placeholderProviders);
        return template;
    }

    private async Task<string?> ResolvePlaceholdersInternalAsync(string? template,
        Func<string?, string?>? defaultTransformer,
        Dictionary<string, string?>? placeholders,
        IProvidesPlaceholders?[] placeholderProviders)
    {
        if (string.IsNullOrWhiteSpace(template)) return template;
        defaultTransformer ??= v => v ?? string.Empty;

        var placeholdersInTemplate = ExtractPlaceholderData(template);
        if (placeholdersInTemplate.Count == 0) return template;

        // Build placeholder dict
        var dict = new Dictionary<string, string?>(placeholders ?? []);
        SetCommonPlaceholders(dict);
        await SetGlobalVariablesAsync(dict);
        if (placeholderProviders != null)
        {
            foreach (var placeholderProvider in placeholderProviders)
            {
                placeholderProvider?.ProvidePlaceholders(dict);
            }
        }

        for (int i = 0; i < 3; i++)
        {
            var before = template;
            template = ResolveDictionaryPlaceholders(template, defaultTransformer, dict, placeholdersInTemplate);
            if (template == before) break;
            placeholdersInTemplate = ExtractPlaceholderData(template);
        }

        return template;
    }

    private List<PlaceholderData> ExtractPlaceholderData(string? template)
    {
        if (string.IsNullOrWhiteSpace(template)) return [];

        var datas = new Dictionary<string, PlaceholderData>();
        foreach (var match in _placeholderRegex.Matches(template).OfType<Match>())
        {
            var data = new PlaceholderData(match.Groups[0].Value, match.Groups["name"].Value,
                match.Groups["transform"].Value);
            datas[data.Raw] = data;
        }

        return datas.Values.ToList();
    }

    private record PlaceholderData(string Raw, string Name, string? TransformerId);

    private string? ResolveDictionaryPlaceholders(string? template, Func<string?, string?>? defaultTransformer,
        Dictionary<string, string?>? values, List<PlaceholderData> placeholdersInTemplate)
    {
        if (values == null || string.IsNullOrWhiteSpace(template)) return template;

        foreach (var placeholder in placeholdersInTemplate)
        {
            if (!values.TryGetValue(placeholder.Name, out string? value)) continue;
            value = TransformValue(value, defaultTransformer, placeholder.TransformerId);
            template = template?.Replace(placeholder.Raw, value, StringComparison.OrdinalIgnoreCase);
        }

        return template;
    }

    private void SetCommonPlaceholders(Dictionary<string, string?> items)
    {
        items["weekday"] = DateTime.Now.DayOfWeek.ToString();
        items["monthName"] = DateTime.Now.ToString("MMMM", _placeholderLocale);
        items["dayOfMonth"] = DateTime.Now.Day.ToString(_placeholderLocale);
        items["hour"] = DateTime.Now.Hour.ToString(_placeholderLocale);
        items["minute"] = DateTime.Now.Minute.ToString(_placeholderLocale);
        items["month"] = DateTime.Now.Month.ToString(_placeholderLocale);
        items["year"] = DateTime.Now.Year.ToString(_placeholderLocale);
        items["guid"] = Guid.NewGuid().ToString();
    }

    private async Task SetGlobalVariablesAsync(Dictionary<string, string?> items)
    {
        if (dbContext == null) return;

        var globalVars = await dbContext.GetWithCacheAsync(x => x.GlobalVariables);
        foreach (var gvar in globalVars)
        {
            if (string.IsNullOrWhiteSpace(gvar.Name) || gvar.Value == null) continue;
            items[gvar.Name] = gvar.Value;
        }
    }

    private string? TransformValue(string? value, Func<string?, string?>? transformer, string? transformerId)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        if (transformerId == "url-encode") value = HttpUtility.UrlEncode(value);
        else if (transformerId == "json-string")
            value = value.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        else if (transformer != null && transformerId != "raw") return transformer(value);

        return value;
    }

    [GeneratedRegex(@"{{(?<name>[\w\-\._]*):?(?<transform>[\w-]+)?}}")]
    private static partial Regex PlaceholderRegex();
}