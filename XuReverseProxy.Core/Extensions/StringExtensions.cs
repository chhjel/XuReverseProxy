namespace XuReverseProxy.Core.Extensions;

public static class StringExtensions
{
    public static string LazyReplace(this string val, string needle, Func<string> replacement, StringComparison stringComparison = StringComparison.Ordinal)
    {
        if (val.Contains(needle, stringComparison)) return val;
        else return val.Replace(needle, replacement(), stringComparison);
    }
}
