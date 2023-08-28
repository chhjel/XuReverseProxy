using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Models.ViewModels.Pages;

public class LoginPageViewModel
{
    public required LoginPageFrontendModel FrontendModel { get; set; }

    [GenerateFrontendModel]
    public class LoginPageFrontendModel
    {
        public string? ServerName { get; set; }
        public string? ReturnUrl { get; set; }
        public string? ErrorCode { get; set; }
        public bool IsRestrictedToLocalhost { get; set; }
        public bool AllowCreateAdmin { get; set; }
        public string? FreshTotpSecret { get; set; }
    }
}
