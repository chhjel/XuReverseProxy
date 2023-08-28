using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text.Json;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Extensions;

public static class HtmlHelperExtensions
{
#pragma warning disable IDE0060 // Remove unused parameter
    public static HtmlString VueComponent<TOptions>(this IHtmlHelper helper, string appName, TOptions options)
    {
        var json = JsonSerializer.Serialize(options, JsonConfig.DefaultOptions);
        return new HtmlString($"<div data-vue-component=\"{appName}\" data-vue-options=\"{WebUtility.HtmlEncode(json)}\"></div>");
    }

    public static HtmlString VueComponentOptions<TOptions>(this IHtmlHelper helper, TOptions options)
    {
        var json = JsonSerializer.Serialize(options ?? new object(), JsonConfig.DefaultOptions);
        return new HtmlString($"data-vue-options=\"{WebUtility.HtmlEncode(json)}\"");
    }
#pragma warning restore IDE0060 // Remove unused parameter
}
