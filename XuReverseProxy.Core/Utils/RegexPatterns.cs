using System.Text.RegularExpressions;

namespace XuReverseProxy.Core.Utils;

public static partial class RegexPatterns
{
    public static readonly Regex CleanUserAgentRegex = MyRegex();

    [GeneratedRegex("^[\\(\\s]*(?<name>[\\p{L}]+)")]
    private static partial Regex MyRegex();
}
