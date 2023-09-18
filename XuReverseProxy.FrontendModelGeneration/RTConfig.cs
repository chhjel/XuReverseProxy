using Microsoft.Extensions.Logging;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Fluent;
using System.Reflection;
using System.Text.RegularExpressions;
using XuReverseProxy.Core.Attributes;
using XuReverseProxy.Core.Models.Config;
using XuReverseProxy.Core.Models.DbEntity;
using XuReverseProxy.Middleware;

namespace XuReverseProxy.FrontendModelGeneration;

public static class RTConfig
{
    public static void Configure(ConfigurationBuilder builder)
    {
        builder.Global((config) =>
        {
            config.CamelCaseForProperties();
            config.UseModules(true);
        });

        // todo: only include needed
        builder.Substitute(typeof(Guid), new RtSimpleTypeName("string"));
        builder.Substitute(typeof(Type), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(MethodInfo), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(PropertyInfo), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(Regex), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(TimeOnly), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(TimeSpan), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(EventId), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(KeyValuePair<,>), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(Stream), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(HttpResponseMessage), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(Exception), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(ProxyClientIdentityData), new RtSimpleTypeName("any"));
        builder.Substitute(typeof(DateTime), new RtSimpleTypeName("Date"));
        builder.Substitute(typeof(DateTimeOffset), new RtSimpleTypeName("Date"));
        builder.Substitute(typeof(List<KeyValuePair<string, string>>), new RtSimpleTypeName("{ [key: string] : string; }"));
        builder.Substitute(typeof(List<KeyValuePair<string, Guid>>), new RtSimpleTypeName("{ [key: string] : string; }"));

        IncludeAssembly(builder, typeof(ServerConfig).Assembly);
        IncludeAssembly(builder, typeof(ReverseProxyMiddleware).Assembly);
    }

    private static void IncludeAssembly(ConfigurationBuilder builder, Assembly assembly)
    {
        var types = assembly.GetExportedTypes()
            .Where(x => x.GetCustomAttribute<GenerateFrontendModelAttribute>() != null)
            .Where(x => x.Namespace?.StartsWith("XuReverseProxy") == true)
            .ToArray();

        builder.ExportAsEnums(types.Where(x => x.IsEnum),
            (config) => ConfigureEnums(config, assembly));
        builder.ExportAsInterfaces(types.Where(x => !x.IsEnum),
            (config) => ConfigureInterfaces(config, assembly));
    }

    private static void ConfigureInterfaces(InterfaceExportBuilder config, Assembly assembly)
    {
        config.AutoI(false);
        config.OverrideNamespace(CreateNamespace("Models.", assembly));
        config.WithAllMethods(m => m.Ignore());
        config.WithAllProperties((c) =>
        {
            if (c.Member.GetCustomAttributes().FirstOrDefault(x => x is GenerateFrontendModelPropertyAttribute) is GenerateFrontendModelPropertyAttribute attr)
            {
                if (attr.ForcedNullable)
                {
                    c.ForceNullable(true);
                }
                if (!string.IsNullOrWhiteSpace(attr.ForcedType))
                {
                    c.Type(attr.ForcedType);
                }
            }
        });
    }

    private static void ConfigureEnums(EnumExportBuilder config, Assembly assembly)
    {
        config.UseString(true);
        config.OverrideNamespace(CreateNamespace("Enums.", assembly));
    }

    private static string CreateNamespace(string prefix, Assembly assembly)
        => $"{prefix}{assembly?.GetName()?.Name?.Replace("XuReverseProxy.", "")?.Replace("XuReverseProxy", "Web") ?? "Other"}";
}
