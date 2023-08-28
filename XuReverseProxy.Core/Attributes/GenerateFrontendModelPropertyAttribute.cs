namespace XuReverseProxy.Core.Attributes;

[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public class GenerateFrontendModelPropertyAttribute : Attribute
{
    public string? ForcedType { get; set; }
    public bool ForcedNullable { get; set; }
}
