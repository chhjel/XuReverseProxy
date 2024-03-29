using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Models.ViewModels.Pages;

namespace XuReverseProxy.Controllers.Pages;

[Authorize]
[Route("[action]")]
public class AdminPageController(IOptionsMonitor<ServerConfig> serverConfig) : Controller
{
    private readonly ServerConfig _serverConfig = serverConfig.CurrentValue;

    [HttpGet("/")]
    public IActionResult Index()
    {
        ViewBag.PageTitle = "Admin";

        var model = new AdminPageViewModel
        {
            FrontendModel = new AdminPageViewModel.AdminPageFrontendModel
            {
                ServerName = _serverConfig.Name ?? "XuReverseProxy",
                RootDomain = _serverConfig.Domain.GetFullDomain(),
                ServerScheme = _serverConfig.Domain.Scheme,
                ServerPort = _serverConfig.Domain.Port,
                ServerDomain = _serverConfig.Domain.Domain,
                DefaultDestinationPrefix = _serverConfig.DefaultDestinationPrefix
            }
        };
        return View(model);
    }
}
