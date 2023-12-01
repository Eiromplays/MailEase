using Crypto = System.Security.Cryptography;

namespace System;

public static class ByteArrayExtensions
{
    private static readonly char[] LowerCaseHexAlphabet = "0123456789abcdef".ToCharArray();
    private static readonly char[] UpperCaseHexAlphabet = "0123456789ABCDEF".ToCharArray();
    private static readonly Crypto.SHA256 Sha256 = Crypto.SHA256.Create();

    public static string? ToHexString(this byte[]? bytes)
    {
        return ToHexString(bytes, true);
    }

    private static string? ToHexString(this IReadOnlyList<byte>? bytes, bool lowerCase)
    {
        if (bytes is null)
            return null;

        var alphabet = lowerCase ? LowerCaseHexAlphabet : UpperCaseHexAlphabet;

        var len = bytes.Count;
        var result = new char[len * 2];

        var i = 0;
        var j = 0;

        while (i < len)
        {
            var b = bytes[i++];
            result[j++] = alphabet[b >> 4];
            result[j++] = alphabet[b & 0xF];
        }

        return new string(result);
    }

    public static byte[]? SHA256(this byte[]? bytes)
    {
        return bytes is null ? null : Sha256.ComputeHash(bytes);
    }
}
