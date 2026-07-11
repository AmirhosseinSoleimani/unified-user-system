
namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public interface IOtpHasher
{
    string Hash(string otpCode, Guid challengeId);
    bool Verify(string otpCode, Guid challengeId, string otpHash);
}
