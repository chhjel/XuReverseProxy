using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XuReverseProxy.Core.Models.Config;

namespace XuReverseProxy.Attributes;

/// <summary>
/// Authorization conditionally enabled based on a given <see cref="RuntimeServerConfig"/> property.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AuthorizeIfEnabledAttribute(string runtimeConfigPropertyName) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context == null) return;

        var config = context.HttpContext.RequestServices.GetService<RuntimeServerConfig>();
        var requireAuth = config?.GetConfigBool(runtimeConfigPropertyName, false) == true;
        if (requireAuth && context.HttpContext?.User?.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(string.Empty);
        }
    }
}
