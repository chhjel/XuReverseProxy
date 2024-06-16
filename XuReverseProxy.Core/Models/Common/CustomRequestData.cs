using System.Text;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.Common;

[GenerateFrontendModel]
public class CustomRequestData
{
    public string? RequestMethod { get; set; }
    public string? Url { get; set; }
    public string? Headers { get; set; }
    public string? Body { get; set; }

    public HttpRequestMessage? CreateRequest(string? url = null)
    {
        // Url
        url ??= Url;
        if (string.IsNullOrWhiteSpace(url)) return null;

        // Method
        var method = HttpMethod.Get;
        if (!string.IsNullOrWhiteSpace(RequestMethod)) method = new HttpMethod(RequestMethod);

        var request = new HttpRequestMessage(method, url);
        
        var headerLines = Headers?.Split('\n').Where(x => x.Contains(':')).Select(x => x.Trim());
        var contentTypeLine =
            headerLines?.FirstOrDefault(x => x.StartsWith("content-type", StringComparison.OrdinalIgnoreCase));
        string contentType = null;
        if (!string.IsNullOrWhiteSpace(contentTypeLine))
        {
            headerLines = headerLines?.Where(x => x != contentTypeLine);
            contentType = contentTypeLine.Split(':', 2)[1].Trim();
        }

        // Body
        if (!string.IsNullOrWhiteSpace(Body))
        {
            if (contentType == null) request.Content = new StringContent(Body);
            else request.Content = new StringContent(Body, Encoding.UTF8, contentType);
        }

        // Headers
        if (headerLines?.Any() == true)
        {
            foreach (var line in headerLines)
            {
                var parts = line.Split(':', 2);
                var name = parts[0].Trim();
                var value = parts[1].Trim();
                request.Headers.TryAddWithoutValidation(name, value);
            }
        }

        return request;
    }
}
