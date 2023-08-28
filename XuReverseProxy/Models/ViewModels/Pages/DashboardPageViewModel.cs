using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Models.ViewModels.Pages;

public class DashboardPageViewModel
{
    public required DashboardPageFrontendModel FrontendModel { get; set; }

    [GenerateFrontendModel]
    public class DashboardPageFrontendModel
    {
    }
}
