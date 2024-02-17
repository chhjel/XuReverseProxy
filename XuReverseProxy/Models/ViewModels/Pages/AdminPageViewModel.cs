using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Models.ViewModels.Pages;

public class AdminPageViewModel
{
    public required AdminPageFrontendModel FrontendModel { get; set; }

    [GenerateFrontendModel]
    public class AdminPageFrontendModel
    {
        public string? ServerName { get; set; }
        public string? RootDomain { get; set; }
        public string? ServerScheme { get; set; }
        public int? ServerPort { get; set; }
        public string? ServerDomain { get; set; }
        public string? DefaultDestinationPrefix { get; set; }
    }
}
