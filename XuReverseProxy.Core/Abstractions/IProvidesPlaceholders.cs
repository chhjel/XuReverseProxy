namespace XuReverseProxy.Core.Abstractions;

public interface IProvidesPlaceholders
{
    void ProvidePlaceholders(Dictionary<string, string?> values);
}
