
namespace UnifiedUserSystem.src.Contracts.DTOs.Auth;

public sealed class VerifyMfaRequest
{
    public Guid ChallengeId { get; set; }
    public string OtpCode { get; set; } = default!;
}
