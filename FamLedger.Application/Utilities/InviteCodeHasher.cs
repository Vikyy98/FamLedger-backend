using System.Security.Cryptography;
using System.Text;

namespace FamLedger.Application.Utilities;

public static class InviteCodeHasher
{
    /// <summary>Normalize user input: trim and uppercase for hashing and comparison.</summary>
    public static string NormalizePlainCode(string? plain)
    {
        if (string.IsNullOrWhiteSpace(plain)) return string.Empty;
        var trimmed = plain.Trim();
        var sb = new StringBuilder(trimmed.Length);
        foreach (var c in trimmed)
        {
            if (char.IsWhiteSpace(c) || c == '-') continue;
            sb.Append(char.ToUpperInvariant(c));
        }
        return sb.ToString();
    }

    public static string Sha256Hex(string normalizedPlain)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedPlain));
        return Convert.ToHexString(bytes);
    }

    /// <summary>Cryptographically random code (A-Z excluding ambiguous, 2-9). 12 characters.</summary>
    public static string GeneratePlainInviteCode()
    {
        const string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var chars = new char[12];
        var bytes = new byte[12];
        RandomNumberGenerator.Fill(bytes);
        for (var i = 0; i < chars.Length; i++)
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        return new string(chars);
    }
}
