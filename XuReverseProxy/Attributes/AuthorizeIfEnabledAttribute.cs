using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XuReverseProxy.Core.Models.Config;

namespace XuReverseProxy.Attributes;

/// <summary>
/// Authorization conditionally enabled based on a given <see cref="RuntimeServerConfig"/> property.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AuthorizeIfEnabledAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _runtimeConfigPropertyName;

    public AuthorizeIfEnabledAttribute(string runtimeConfigPropertyName)
    {
        _runtimeConfigPropertyName = runtimeConfigPropertyName;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context == null) return;

        var config = context.HttpContext.RequestServices.GetService<RuntimeServerConfig>();
        var requireAuth = config?.GetConfigBool(_runtimeConfigPropertyName, false) == true;
        if (requireAuth && context.HttpContext?.User?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(string.Empty);
        }
    }
}
