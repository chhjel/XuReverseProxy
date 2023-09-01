using System.Text.Json;
using System.Text.Json.Serialization;

namespace XuReverseProxy.Core.Utils;

public static class JsonConfig
{
    private static JsonSerializerOptions? _jsonSerializerOptionsCache;

    public static JsonSerializerOptions DefaultOptions
    {
        get
        {
            if (_jsonSerializerOptionsCache == null)
            {
                _jsonSerializerOptionsCache = new JsonSerializerOptions();
                ApplyDefaultOptions(_jsonSerializerOptionsCache);
            }
            return _jsonSerializerOptionsCache;
        }
    }

    public static JsonSerializerOptions ApplyDefaultOptions(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
