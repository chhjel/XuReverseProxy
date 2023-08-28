﻿using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace XuReverseProxy.Core.Utils;

/// <summary>
/// https://raw.githubusercontent.com/dotnet/aspnetcore/main/src/Identity/Extensions.Core/src/Rfc6238AuthenticationService.cs
/// </summary>
public static class Rfc6238AuthenticationService
{
    private static readonly TimeSpan _timestep = TimeSpan.FromMinutes(3);
    private static readonly Encoding _encoding = new UTF8Encoding(false, true);

    internal static int ComputeTotp(
        byte[] key,
        ulong timestepNumber,
        byte[]? modifierBytes)
    {
        // # of 0's = length of pin
        const int Mod = 1000000;

        // See https://tools.ietf.org/html/rfc4226
        // We can add an optional modifier
        Span<byte> timestepAsBytes = stackalloc byte[sizeof(long)];
        var res = BitConverter.TryWriteBytes(timestepAsBytes, IPAddress.HostToNetworkOrder((long)timestepNumber));
        Debug.Assert(res);

        Span<byte> modifierCombinedBytes = timestepAsBytes;
        if (modifierBytes is not null)
        {
            modifierCombinedBytes = ApplyModifier(timestepAsBytes, modifierBytes);
        }
        Span<byte> hash = stackalloc byte[HMACSHA1.HashSizeInBytes];
        res = HMACSHA1.TryHashData(key, modifierCombinedBytes, hash, out var written);
        Debug.Assert(res);
        Debug.Assert(written == hash.Length);

        // Generate DT string
        var offset = hash[^1] & 0xf;
        Debug.Assert(offset + 4 < hash.Length);
        var binaryCode = (hash[offset] & 0x7f) << 24
                            | (hash[offset + 1] & 0xff) << 16
                            | (hash[offset + 2] & 0xff) << 8
                            | (hash[offset + 3] & 0xff);

        return binaryCode % Mod;
    }

    private static byte[] ApplyModifier(Span<byte> input, byte[] modifierBytes)
    {
        var combined = new byte[checked(input.Length + modifierBytes.Length)];
        input.CopyTo(combined);
        Buffer.BlockCopy(modifierBytes, 0, combined, input.Length, modifierBytes.Length);
        return combined;
    }

    // More info: https://tools.ietf.org/html/rfc6238#section-4
    private static ulong GetCurrentTimeStepNumber()
    {
        var delta = DateTimeOffset.UtcNow - DateTimeOffset.UnixEpoch;
        return (ulong)(delta.Ticks / _timestep.Ticks);
    }

    public static int GenerateCode(byte[] securityToken, string? modifier = null)
    {
        // Allow a variance of no greater than 9 minutes in either direction
        var currentTimeStep = GetCurrentTimeStepNumber();

        var modifierBytes = modifier is not null ? _encoding.GetBytes(modifier) : null;
        return ComputeTotp(securityToken, currentTimeStep, modifierBytes);
    }

    public static bool ValidateCode(byte[] securityToken, int code, string? modifier = null)
    {
        // Allow a variance of no greater than 9 minutes in either direction
        var currentTimeStep = GetCurrentTimeStepNumber();
        var modifierBytes = modifier is not null ? _encoding.GetBytes(modifier) : null;
        for (var i = -2; i <= 2; i++)
        {
            var computedTotp = ComputeTotp(securityToken, (ulong)((long)currentTimeStep + i), modifierBytes);
            if (computedTotp == code)
            {
                return true;
            }
        }

        // No match
        return false;
    }
}
