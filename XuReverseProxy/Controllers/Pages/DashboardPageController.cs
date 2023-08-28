using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XuReverseProxy.Models.ViewModels.Pages;

namespace XuReverseProxy.Controllers.Pages;

[Authorize]
[Route("[action]")]
public class DashboardPageController : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        ViewBag.PageTitle = "Dashboard";

        var model = new DashboardPageViewModel
        {
            FrontendModel = new DashboardPageViewModel.DashboardPageFrontendModel
            {

            }
        };
        return View(model);
    }
}
