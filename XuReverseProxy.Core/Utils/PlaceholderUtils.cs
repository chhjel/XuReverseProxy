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

    public static string? ResolveCommonPlaceholders(string? template, Func<string?, string?>? transformer = null)
    {
        string mod(string val) => transformer?.Invoke(val) ?? val;

        return template
            ?.Replace("{{weekday}}", mod(DateTime.Now.DayOfWeek.ToString()), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{monthName}}", mod(DateTime.Now.ToString("MMMM")), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{dayOfMonth}}", mod(DateTime.Now.Day.ToString()), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{hour}}", mod(DateTime.Now.Hour.ToString()), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{minute}}", mod(DateTime.Now.Minute.ToString()), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{month}}", mod(DateTime.Now.Month.ToString()), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{year}}", mod(DateTime.Now.Year.ToString()), StringComparison.OrdinalIgnoreCase);
    }
}
