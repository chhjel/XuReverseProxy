using XuReverseProxy.Core.Abstractions;

namespace XuReverseProxy.Core.Utils;

public static class PlaceholderUtils
{
    public static string ResolvePlaceholders(string template, params IProvidesPlaceholders?[] placeholderProviders)
    {
        template = ResolveCommonPlaceholders(template)!;
        foreach (var  placeholderProvider in placeholderProviders)
        {
            if (placeholderProvider != null) template = placeholderProvider.ResolvePlaceholders(template);
        }
        return template;
    }

    public static string? ResolveCommonPlaceholders(string? template)
    {
        return template
            ?.Replace("{{weekday}}", DateTime.Now.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{monthName}}", DateTime.Now.ToString("MMMM"), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{dayOfMonth}}", DateTime.Now.Day.ToString(), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{hour}}", DateTime.Now.Hour.ToString(), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{minute}}", DateTime.Now.Minute.ToString(), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{month}}", DateTime.Now.Month.ToString(), StringComparison.OrdinalIgnoreCase)
            ?.Replace("{{year}}", DateTime.Now.Year.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
