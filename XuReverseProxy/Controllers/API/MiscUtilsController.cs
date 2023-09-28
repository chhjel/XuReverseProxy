using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QoDL.Toolkit.Core.Util;
using System.Text.RegularExpressions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Controllers.API;

[Authorize]
[Route("/api/[controller]")]
public class MiscUtilsController : Controller
{
    [HttpPost("IsIPInCidrRange")]
    public bool IsIPInCidrRange([FromBody] TestCidrRangeRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.IP)) return false;
        return TKIPAddressUtils.IpMatchesOrIsWithinCidrRange(request.IP, request.IPCidr);
    }
    [GenerateFrontendModel]
    public record TestCidrRangeRequestModel(string IP, string IPCidr);

    [HttpPost("TestRegex")]
    public bool TestRegex([FromBody] TestRegexRequestModel request)
    {
        if (request.Pattern == null) return false;
        var regex = new Regex(request.Pattern, RegexOptions.IgnoreCase);
        return regex.IsMatch(request.Input);
    }
    [GenerateFrontendModel]
    public record TestRegexRequestModel(string Input, string Pattern);
}
