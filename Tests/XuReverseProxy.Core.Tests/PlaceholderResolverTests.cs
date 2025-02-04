using System.Web;
using XuReverseProxy.Core.Services;

namespace XuReverseProxy.Core.Tests;

public class PlaceholderResolverTests
{
    [Fact]
    public async Task ResolvePlaceholders_WithNoTransformers_Works()
    {
        var template = "Url is {{url}}";
        var expected = $"Url is {placeholders["url"]}";
        var resolver = new PlaceholderResolver(null);
        var resolved = await resolver.ResolvePlaceholdersAsync(template, null, placeholders, []);
        Assert.Equal(expected, resolved);
    }

    [Fact]
    public async Task ResolvePlaceholders_WithDefaultTransformers_Works()
    {
        var template = "Url is {{url}}";
        var expected = $"Url is {HttpUtility.UrlEncode(placeholders["url"])}";
        var resolver = new PlaceholderResolver(null);
        var resolved = await resolver.ResolvePlaceholdersAsync(template, HttpUtility.UrlEncode, placeholders, []);
        Assert.Equal(expected, resolved);
    }

    [Fact]
    public async Task ResolvePlaceholders_WithSpecifiedNoTransformers_Works()
    {
        var template = "Url is {{url:raw}}";
        var expected = $"Url is {placeholders["url"]}";
        var resolver = new PlaceholderResolver(null);
        var resolved = await resolver.ResolvePlaceholdersAsync(template, HttpUtility.UrlEncode, placeholders, []);
        Assert.Equal(expected, resolved);
    }

    [Fact]
    public async Task ResolvePlaceholders_WithSpecifiedUrlEncodeTransformers_Works()
    {
        var template = "Url is {{url:url-encode}}";
        var expected = $"Url is {HttpUtility.UrlEncode(placeholders["url"])}";
        var resolver = new PlaceholderResolver(null);
        var resolved = await resolver.ResolvePlaceholdersAsync(template, null, placeholders, []);
        Assert.Equal(expected, resolved);
    }

    [Fact]
    public async Task ResolvePlaceholders_WithSpecifiedJsonStringTransformers_Works()
    {
        var template = "WithQuotes is {{WithQuotes:json-string}}";
        var expected = "WithQuotes is This \\\"thing\\\" has quotes in it.";
        var resolver = new PlaceholderResolver(null);
        var resolved = await resolver.ResolvePlaceholdersAsync(template, null, placeholders, []);
        Assert.Equal(expected, resolved);
    }

    private static Dictionary<string, string?> placeholders =
        new() { { "url", "https://www.test.com" }, { "WithQuotes", "This \"thing\" has quotes in it." } };
}