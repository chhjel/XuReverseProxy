using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class HtmlTemplate : IHasId
{
    public Guid Id { get; set; }
    public HtmlTemplateType Type { get; set; }
    public int ResponseCode { get; set; }
    public string? Html { get; set; }

    public static readonly HtmlTemplate FallbackHtmlTemplate = new()
    {
        Html = $"<!DOCTYPE html>\n<html>\n<head>\n<title>XuReverseProxy</title>\n</head>\n<body>\n</body>\n</html>\n",
        ResponseCode = 404
    };

    public static Dictionary<HtmlTemplateType, HtmlTemplate> HtmlTemplateDefaults { get; } = new()
    {
        { HtmlTemplateType.ProxyNotFound, CreateInitialValue(HtmlTemplateType.ProxyNotFound) },
        { HtmlTemplateType.ClientBlocked, CreateInitialValue(HtmlTemplateType.ClientBlocked) },
        { HtmlTemplateType.IPBlocked, CreateInitialValue(HtmlTemplateType.IPBlocked) },
        { HtmlTemplateType.ProxyConditionsNotMet, CreateInitialValue(HtmlTemplateType.ProxyConditionsNotMet) }
    };

    private static HtmlTemplate CreateInitialValue(HtmlTemplateType type)
    {
        if (type == HtmlTemplateType.ProxyNotFound)
        {
            return new()
            {
                Type = type,
                Html = "<!DOCTYPE html>\n<html>\n<head>\n<title>404 | XuReverseProxy</title>\n</head>\n<body>\n404\n</body>\n</html>\n",
                ResponseCode = 404
            };
        }
        else if (type == HtmlTemplateType.ClientBlocked)
        {
            return new()
            {
                Type = type,
                Html = "<!DOCTYPE html>\n<html>\n<head>\n<title>Blocked | XuReverseProxy</title>\n</head>\n<body>\n{{Client.BlockedMessage}}\n</body>\n</html>\n",
                ResponseCode = 401
            };
        }
        else if (type == HtmlTemplateType.IPBlocked)
        {
            return new()
            {
                Type = type,
                Html = "<!DOCTYPE html>\n<html>\n<head>\n<title>Blocked | XuReverseProxy</title>\n</head>\n<body>\n❌\n</body>\n</html>\n",
                ResponseCode = 401
            };
        }
        else if (type == HtmlTemplateType.ProxyConditionsNotMet)
        {
            return new()
            {
                Type = type,
                Html = "<!DOCTYPE html>\n<html>\n<head>\n<title>404 | XuReverseProxy</title>\n</head>\n<body>\n404\n</body>\n</html>\n",
                ResponseCode = 404
            };
        }

        return new()
        {
            Type = type,
            Html = $"<!DOCTYPE html>\n<html>\n<head>\n<title>{type} | XuReverseProxy</title>\n</head>\n<body>\n${type}</body>\n</html>\n",
            ResponseCode = 404
        };
    }
}

[GenerateFrontendModel]
public enum HtmlTemplateType
{
    ProxyNotFound = 0,
    ClientBlocked = 1,
    IPBlocked = 2,
    ProxyConditionsNotMet = 3
}
