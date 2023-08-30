using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Models.ViewModels.Pages;

public class AdminPageViewModel
{
    public required AdminPageFrontendModel FrontendModel { get; set; }

    [GenerateFrontendModel]
    public class AdminPageFrontendModel
    {
        public string? Etc { get; set; }
    }
}
