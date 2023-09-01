using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Models.ViewModels.Pages;

namespace XuReverseProxy.Controllers.Pages;

[Authorize]
[Route("[action]")]
public class AdminPageController : Controller
{
    private readonly IOptionsMonitor<ServerConfig> _serverConfig;

    public AdminPageController(IOptionsMonitor<ServerConfig> serverConfig)
    {
        _serverConfig = serverConfig;
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        ViewBag.PageTitle = "Admin";

        var model = new AdminPageViewModel
        {
            FrontendModel = new AdminPageViewModel.AdminPageFrontendModel
            {
                RootDomain = _serverConfig.CurrentValue.Domain.GetFullDomain()
            }
        };
        return View(model);
    }
}
