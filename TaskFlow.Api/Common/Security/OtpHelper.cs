using System.Security.Cryptography;
using System.Text;

namespace TaskFlow.Api.Common.Security;

public static class OtpHelper
{
    public const int CodeLength = 6;
    public const int HashLength = 64;

    public static string GenerateCode()
    {
        return RandomNumberGenerator.GetInt32(1_000_000).ToString("D6");
    }

    public static string HashCode(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool IsMatch(string stored, string input)
    {
        if (string.IsNullOrEmpty(stored) || string.IsNullOrEmpty(input))
            return false;

        if (stored.Length == CodeLength)
            return string.Equals(stored, input, StringComparison.Ordinal);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(stored),
            Encoding.UTF8.GetBytes(HashCode(input)));
    }
}