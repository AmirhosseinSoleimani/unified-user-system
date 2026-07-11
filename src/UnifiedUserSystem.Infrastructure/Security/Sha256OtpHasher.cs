
using System.Security.Cryptography;
using System.Text;
using UnifiedUserSystem.src.Application.Abstractions.Security;

namespace UnifiedUserSystem.src.Infrastructure.Security;

public sealed class Sha256OtpHasher : IOtpHasher
{
    public string Hash(string otpCode, Guid challengeId)
    {
        if (string.IsNullOrWhiteSpace(otpCode))
            throw new ArgumentException("OTP code is required.", nameof(otpCode));

        var normalized = otpCode.Trim();
        var payload = $"{challengeId:N}:{normalized}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public bool Verify(string otpCode, Guid challengeId, string otpHash)
    {
        if (string.IsNullOrWhiteSpace(otpHash))
            return false;

        var computed = Hash(otpCode, challengeId);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(otpHash.Trim().ToLowerInvariant()));
    }
}
