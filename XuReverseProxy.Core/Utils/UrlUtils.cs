namespace XuReverseProxy.Core.Utils;

public static class UrlUtils
{
    public static string? ChangeUrlAuthorityAndPort(string? url, string? newAuthorityAndPort)
    {
        if (string.IsNullOrWhiteSpace(newAuthorityAndPort) || string.IsNullOrWhiteSpace(url)) return url;
        var builder = new UriBuilder(url);

        var newIsRelative = newAuthorityAndPort.Trim().StartsWith('/');
        if (!newIsRelative && !IsValidHttpUri(newAuthorityAndPort))
        {
            newAuthorityAndPort = $"{builder.Scheme}://{newAuthorityAndPort}";
        }

        var newValues = new Uri(newAuthorityAndPort, newIsRelative ? UriKind.Relative : UriKind.Absolute);

        builder.Scheme = newIsRelative ? builder.Scheme : newValues.Scheme;
        builder.Host = newIsRelative ? builder.Host : newValues.Host;
        builder.Port = newIsRelative ? builder.Port : newValues.Port;

        return builder.Uri.ToString();
    }

    private static bool IsValidHttpUri(string uriString) 
       => Uri.TryCreate(uriString, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
