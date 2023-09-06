namespace XuReverseProxy.Core.Abstractions;

public interface IProvidesPlaceholders
{
    string ResolvePlaceholders(string template, Func<string?, string?> transformer);
}
