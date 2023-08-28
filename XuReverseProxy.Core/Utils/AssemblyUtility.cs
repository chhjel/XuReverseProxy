using System.Reflection;

namespace XuReverseProxy.Core.Utils;

public static class AssemblyUtility
{
    public static Version GetVersion() => Assembly.GetExecutingAssembly()?.GetName()?.Version ?? new Version(1, 0);
}
