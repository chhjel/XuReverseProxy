using QoDL.Toolkit.Core.Util;
using System.Reflection;
using System.Text.Json;
using XuReverseProxy.Core.ProxyAuthentication.Attributes;
using XuReverseProxy.Core.ProxyAuthentication.Challenges;
using XuReverseProxy.Core.Utils;

namespace XuReverseProxy.Core.ProxyAuthentication;

public static class InvokableProxyAuthMethodUtils
{
    private static List<InvokableProxyAuthMethodDefinition>? _methodDefs = null;
    private static readonly object _methodDefsCacheLock = new();

    public static async Task<object?> InvokeMethodAsync(object instance, string methodName, string jsonPayload, ProxyChallengeInvokeContext context)
    {
        var instanceType = instance.GetType();
        var methodDef = GetMethodDefinitions().FirstOrDefault(x => x.Type == instanceType && x.MethodInfo.Name == methodName);
        if (methodDef == null) return null;
        return await methodDef.InvokeMethodAsync(instance, jsonPayload, context);
    }

    public static List<InvokableProxyAuthMethodDefinition> GetMethodDefinitions()
    {
        EnsureMethodsDiscovered(typeof(InvokableProxyAuthMethodUtils).Assembly);
        return _methodDefs!;
    }

    private static void EnsureMethodsDiscovered(params Assembly[] assemblies)
    {
        lock (_methodDefsCacheLock)
        {
            if (_methodDefs != null) return;

            _methodDefs = new();
            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.ExportedTypes)
                {
                    if (!type.IsAssignableTo(typeof(ProxyChallengeTypeBase))) continue;

                    foreach (var method in type.GetMethods())
                    {
                        var attr = method.GetCustomAttribute<InvokableProxyAuthMethodAttribute>();
                        if (attr == null) continue;

                        var methodDef = CreateMethodDefinition(attr, method, type);
                        _methodDefs.Add(methodDef);
                    }
                }
            }
        }
    }

    private static InvokableProxyAuthMethodDefinition CreateMethodDefinition(InvokableProxyAuthMethodAttribute attr, MethodInfo method, Type type)
        => new(type, method, attr);

    public class InvokableProxyAuthMethodDefinition
    {
        public Type Type { get; }
        public MethodInfo MethodInfo { get; }
        public InvokableProxyAuthMethodAttribute Attribute { get; }
        public Type PayloadType { get; }

        public InvokableProxyAuthMethodDefinition(Type type, MethodInfo methodInfo, InvokableProxyAuthMethodAttribute attribute)
        {
            Type = type;
            MethodInfo = methodInfo;
            Attribute = attribute;

            var parameters = MethodInfo.GetParameters();
            if (parameters.Length != 2) throw new ArgumentException($"Method '{methodInfo.Name}' on type '{type.Name}' must have 2 parameters.", nameof(methodInfo));
            else if (parameters[0].ParameterType != typeof(ProxyChallengeInvokeContext)) throw new ArgumentException($"Method '{methodInfo.Name}' on type '{type.Name}' must have 2 parameters. The first must be of type ProxyChallengeInvokeContext.", nameof(methodInfo));

            PayloadType = parameters[1].ParameterType;
        }

        public async Task<object?> InvokeMethodAsync(object instance, string jsonPayload, ProxyChallengeInvokeContext context)
        {
            var payload = string.IsNullOrWhiteSpace(jsonPayload)
                ? Activator.CreateInstance(PayloadType)
                : JsonSerializer.Deserialize(jsonPayload, PayloadType, JsonConfig.DefaultOptions);
            var parameters = new object[] { context, payload! };
            return await TKAsyncUtils.InvokeAsync(MethodInfo, instance, parameters);
        }
    }
}
