namespace XuReverseProxy.Core.Utils;

public static class TotpUtils
{
    public static string GenerateNewKey() => Base32.GenerateBase32();

    /// <summary>
    /// Validate code without any edge handling, the code will only work within a single timestep.
    /// </summary>
    public static bool ValidateCode(string? secret, string? token)
    {
        if (secret == null || !int.TryParse(token, out var code))
        {
            return false;
        }

        var keyBytes = Base32.FromBase32(secret);
        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestep = Convert.ToInt64(unixTimestamp / 30);
        var expectedCode = Rfc6238AuthenticationService.ComputeTotp(keyBytes, (ulong)timestep, modifierBytes: null);
        return expectedCode == code;
    }
}
