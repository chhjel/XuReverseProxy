using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XuReverseProxy.Models.ViewModels.Pages;

namespace XuReverseProxy.Controllers.Pages;

[Authorize]
[Route("[action]")]
public class AdminPageController : Controller
{
    [HttpGet("/")]
    public IActionResult Index()
    {
        ViewBag.PageTitle = "Admin";

        var model = new AdminPageViewModel
        {
            FrontendModel = new AdminPageViewModel.AdminPageFrontendModel
            {
                Etc = "asd"
            }
        };
        return View(model);
    }
}
