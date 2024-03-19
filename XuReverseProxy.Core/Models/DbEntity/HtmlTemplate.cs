using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class HtmlTemplate
{
    public Guid Id { get; set; }
    public HtmlTemplateType Type { get; set; }
    public int ResponseCode { get; set; }
    public string? Template { get; set; }

    public static HtmlTemplate? CreateInitialValue(HtmlTemplateType type)
    {
        if (type == HtmlTemplateType.ProxyNotFound)
        {
            return new()
            {
                Type = type,
                Template = "<!DOCTYPE html>\n<html>\n<head>\n<title>404 | XuReverseProxy</title>v</head>\n<body>\n404 / XuReverseProxy\n</body>\n</html>\n",
                ResponseCode = 404,
            };
        }
        else if (type == HtmlTemplateType.ClientBlocked)
        {
            return new()
            {
                Type = type,
                Template = "<!DOCTYPE html>\n<html>\n<head>\n<title>Blocked | XuReverseProxy</title>\n</head>\n<body>\n{{blocked_message}}\n</body>\n</html>\n",
                ResponseCode = 401,
            };
        }
        else if (type == HtmlTemplateType.IPBlocked)
        {
            return new()
            {
                Type = type,
                Template = "<!DOCTYPE html>\n<html>\n<head>\n<title>Blocked | XuReverseProxy</title>\n</head>\n<body>\nNope</body>\n</html>\n",
                ResponseCode = 401,
            };
        }
        else if (type == HtmlTemplateType.ProxyConditionsNotMet)
        {
            // No defaults yet
            return null;
        }
        return null;
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
