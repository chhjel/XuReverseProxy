using Microsoft.AspNetCore.Http;

namespace XuReverseProxy.Core.Extensions;

public static class HttpRequestExtensions
{
    public static async Task<string> ReadBodyAsStringAsync(this HttpRequest request)
    {
        using var reader = new StreamReader(request.Body);
        return await reader.ReadToEndAsync();
    }
}
