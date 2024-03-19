using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Net.Http.Json;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Services;

public interface IIPLookupService
{
    Task<IPLookupResult?> LookupIPAsync(string? ip);
}

public class IPWhoIsIPLookupService(IHttpClientFactory httpClientFactory, IMemoryCache cache) : IIPLookupService
{
    public async Task<IPLookupResult?> LookupIPAsync(string? ip)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ip) || ip == "localhost") return new();
            if (cache.TryGetValue<IPLookupResult>(ip, out var cached)) return cached;

            var httpClient = httpClientFactory.CreateClient();
            var url = $"http://ipwho.is/{ip}";
            var response = await httpClient.GetFromJsonAsync<IpWhoIsLookupResult>(url);

            var result = new IPLookupResult
            {
                Success = !string.IsNullOrWhiteSpace(response?.Ip) && response?.Success == true,
                IP = response?.Ip,
                Continent = response?.Continent,
                Country = response?.Country,
                City = response?.City,
                Latitude = response?.Latitude,
                Longitude = response?.Longitude,
                FlagUrl = response?.Flag?.Img?.ToString()
            };
            cache.Set(ip, result, TimeSpan.FromMinutes(5));
            return result;
        }
        catch (Exception)
        {
            return new();
        }
    }

    #region Models
    private class IpWhoIsLookupResult
    {
        [JsonProperty("ip")]
        public string? Ip { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("continent")]
        public string? Continent { get; set; }

        [JsonProperty("continent_code")]
        public string? ContinentCode { get; set; }

        [JsonProperty("country")]
        public string? Country { get; set; }

        [JsonProperty("country_code")]
        public string? CountryCode { get; set; }

        [JsonProperty("region")]
        public string? Region { get; set; }

        [JsonProperty("region_code")]
        public string? RegionCode { get; set; }

        [JsonProperty("city")]
        public string? City { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("is_eu")]
        public bool IsEu { get; set; }

        [JsonProperty("postal")]
        public string? Postal { get; set; }

        [JsonProperty("calling_code")]
        public string? CallingCode { get; set; }

        [JsonProperty("capital")]
        public string? Capital { get; set; }

        [JsonProperty("borders")]
        public string? Borders { get; set; }

        [JsonProperty("flag")]
        public IpWhoIsLookupFlagResult? Flag { get; set; }

        [JsonProperty("connection")]
        public IpWhoIsLookupConnectionResult? Connection { get; set; }

        [JsonProperty("timezone")]
        public IpWhoIsLookupTimezoneResult? Timezone { get; set; }
    }

    private class IpWhoIsLookupConnectionResult
    {
        [JsonProperty("asn")]
        public long? Asn { get; set; }

        [JsonProperty("org")]
        public string? Org { get; set; }

        [JsonProperty("isp")]
        public string? Isp { get; set; }

        [JsonProperty("domain")]
        public string? Domain { get; set; }
    }

    private partial class IpWhoIsLookupFlagResult
    {
        [JsonProperty("img")]
        public Uri? Img { get; set; }

        [JsonProperty("emoji")]
        public string? Emoji { get; set; }

        [JsonProperty("emoji_unicode")]
        public string? EmojiUnicode { get; set; }
    }

    private partial class IpWhoIsLookupTimezoneResult
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("abbr")]
        public string? Abbr { get; set; }

        [JsonProperty("is_dst")]
        public bool IsDst { get; set; }

        [JsonProperty("offset")]
        public long Offset { get; set; }

        [JsonProperty("utc")]
        public string? Utc { get; set; }

        [JsonProperty("current_time")]
        public DateTimeOffset CurrentTime { get; set; }
    }
    #endregion
}

[GenerateFrontendModel]
public class IPLookupResult
{
    public bool Success { get; set; }
    public string? IP { get; set; }
    public string? Continent { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? FlagUrl { get; set; }
}
