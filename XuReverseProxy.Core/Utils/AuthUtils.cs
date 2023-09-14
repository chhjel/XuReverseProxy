namespace XuReverseProxy.Core.Utils;

public static class AuthUtils
{
    private static readonly Random _random = new();

    // Delay a bit to make timing attacks harder
    public static async Task RandomAuthDelay() => await Task.Delay(_random.Next(250, 750));
}