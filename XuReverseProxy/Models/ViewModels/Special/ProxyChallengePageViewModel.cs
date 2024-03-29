﻿using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.DbEntity;

namespace XuReverseProxy.Models.ViewModels.Special;

[GenerateFrontendModel]
public class ProxyChallengePageFrontendModel
{
    public required string Title { get; set; }
    public required string? Description { get; set; }

    public List<AuthWithUnfulfilledConditions> AuthsWithUnfulfilledConditions { get; set; } = [];
    public List<ChallengeModel> ChallengeModels { get; set; } = [];

    [GenerateFrontendModel]
    public readonly record struct AuthWithUnfulfilledConditions(string TypeId, List<AuthCondition> Conditions);
    
    [GenerateFrontendModel]
    public readonly record struct ChallengeModel(Guid AuthId, string TypeId, int Order, bool Solved, object FrontendModel, List<AuthCondition> Conditions);
    
    [GenerateFrontendModel]
    public readonly record struct AuthCondition(ConditionData.ConditionType Type, int Group, string Summary, bool Passed);
}
