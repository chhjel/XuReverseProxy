using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Services;

namespace XuReverseProxy.Models.ViewModels.Pages;

public class ManualApprovalProxyAuthPageViewModel
{
    public required ManualApprovalProxyAuthPageFrontendModel FrontendModel { get; set; }

    [GenerateFrontendModel]
    public class ManualApprovalProxyAuthPageFrontendModel
    {
        public required Guid AuthenticationId { get; set; }
        public required Guid SolvedId { get; set; }
        public required bool IsLoggedIn { get; set; }
        public required ClientDataFrontendModel Client { get; set; }
        public required CurrentChallengeDataFrontendModel CurrentChallengeData { get; set; }
        public required List<ChallengeDataFrontendModel> AllChallengeData { get; set; }
        public required ProxyConfigFrontendModel ProxyConfig { get; set; }
        public required bool IsApproved { get; set; }
        public required string Url { get; set; }

        [GenerateFrontendModel]
        public class ClientDataFrontendModel
        {
            public required Guid Id { get; set; }
            public required string? IP { get; set; }
            public required IPLookupResult? IPLocation { get; set; }
            public required string? UserAgent { get; set; }
            public required bool Blocked { get; set; }
            public required DateTime? BlockedAtUtc { get; set; }
            public required string? BlockedMessage { get; set; }
            public required DateTime LastAttemptedAccessedAtUtc { get; set; }
            public required DateTime CreatedAtUtc { get; set; }
            public required DateTime? LastAccessedAtUtc { get; set; }
            public required string? Note { get; set; }
        }

        [GenerateFrontendModel]
        public class CurrentChallengeDataFrontendModel
        {
            public required string? EasyCode { get; set; }
            public required DateTime RequestedAt { get; set; }
        }

        [GenerateFrontendModel]
        public class ChallengeDataFrontendModel
        {
            public required string? Type { get; set; }
            public required TimeSpan? SolvedDuration { get; set; }
            public required bool Solved { get; set; }
            public required DateTime? SolvedAtUtc { get; set; }
            public required bool ConditionsNotMet { get; set; }
        }

        [GenerateFrontendModel]
        public class ProxyConfigFrontendModel
        {
            public required string? Name { get; set; }
            public required bool Enabled { get; set; }
            public required string? Subdomain { get; set; }
            public required int? Port { get; set; }
            public required string? Destination { get; set; }
            public required string? ChallengeTitle { get; set; }
        }
    }
}
