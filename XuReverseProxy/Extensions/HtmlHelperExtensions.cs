using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net;
using System.Text.Json;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Extensions;

public static class HtmlHelperExtensions
{
    public static HtmlString VueComponent<TOptions>(this IHtmlHelper _, string appName, TOptions options)
    {
        var json = JsonSerializer.Serialize(options, JsonConfig.DefaultOptions);
        return new HtmlString($"<div data-vue-component=\"{appName}\" data-vue-options=\"{WebUtility.HtmlEncode(json)}\"></div>");
    }

    public static HtmlString VueComponentOptions<TOptions>(this IHtmlHelper _, TOptions options)
    {
        var json = JsonSerializer.Serialize(options ?? new object(), JsonConfig.DefaultOptions);
        return new HtmlString($"data-vue-options=\"{WebUtility.HtmlEncode(json)}\"");
    }
}
