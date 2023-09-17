using System.Globalization;
using XuReverseProxy.Core.Abstractions;

namespace XuReverseProxy.Core.Utils;

public static class PlaceholderUtils
{
    public static string? ResolvePlaceholders(string? template, params IProvidesPlaceholders?[] placeholderProviders)
        => ResolvePlaceholders(template, null, placeholderProviders);

    public static string? ResolvePlaceholders(string? template, Func<string?, string?>? transformer = null, params IProvidesPlaceholders?[] placeholderProviders)
    {
        if (string.IsNullOrWhiteSpace(template)) return template;
        transformer ??= v => v ?? string.Empty;

        template = ResolveCommonPlaceholders(template)!;
        foreach (var placeholderProvider in placeholderProviders)
        {
            if (placeholderProvider != null) template = placeholderProvider.ResolvePlaceholders(template, transformer);
        }
        return template;
    }

    public static string? ResolvePlaceholders(string? template, Dictionary<string, string?> placeholders)
        => ResolvePlaceholders(template, null, placeholders);

    public static string? ResolvePlaceholders(string? template, Func<string?, string?>? transformer, Dictionary<string, string?> placeholders)
    {
        if (string.IsNullOrWhiteSpace(template)) return template;
        transformer ??= v => v ?? string.Empty;
        string mod(string val) => transformer?.Invoke(val) ?? val;

        foreach (var placeholder in placeholders)
        {
            template = template?.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value == null ? string.Empty : mod(placeholder.Value), StringComparison.OrdinalIgnoreCase);
        }
        return template;
    }

    private static readonly CultureInfo _placeholderLocale = CultureInfo.CreateSpecificCulture("en");
    public static string? ResolveCommonPlaceholders(string? template, Func<string?, string?>? transformer = null)
    {
        string mod(string val) => transformer?.Invoke(val) ?? val;

        return template
            ?.Replace("{{weekday}}", mod(DateTime.Now.DayOfWeek.ToString()), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{monthName}}", mod(DateTime.Now.ToString("MMMM", _placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{dayOfMonth}}", mod(DateTime.Now.Day.ToString(_placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{hour}}", mod(DateTime.Now.Hour.ToString(_placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{minute}}", mod(DateTime.Now.Minute.ToString(_placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{month}}", mod(DateTime.Now.Month.ToString(_placeholderLocale)), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{year}}", mod(DateTime.Now.Year.ToString(_placeholderLocale)), StringComparison.OrdinalIgnoreCase);
    }
}
