﻿using System.Text.Json.Serialization;
using XuReverseProxy.Core.Abstractions;
using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.DbEntity;

[GenerateFrontendModel]
public class ProxyClientIdentityData : IHasId
{
    public Guid Id { get; set; }
    public Guid IdentityId { get; set; }
    public Guid? AuthenticationId { get; set; }
    [JsonIgnore]
    public ProxyClientIdentity Identity { get; set; } = null!;
    public required string Key { get; set; }
    public string? Value { get; set; }
}
