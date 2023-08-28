using Microsoft.AspNetCore.Mvc;

namespace XuReverseProxy.Controllers.Pages;

public class ErrorPageController : Controller
{
    [HttpGet("/error/{statusCode}")]
    public IActionResult Index(int statusCode)
    {
        ViewBag.PageTitle = $"{statusCode}";
        return View(statusCode);
    }
}
