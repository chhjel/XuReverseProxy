using System.Text.Json;
using System.Text.Json.Serialization;

namespace XuReverseProxy.Core.Utils;

public static class JsonConfig
{
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };
}
