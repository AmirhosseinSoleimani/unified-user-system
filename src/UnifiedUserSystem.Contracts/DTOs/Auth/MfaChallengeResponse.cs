
namespace UnifiedUserSystem.src.Contracts.DTOs.Auth;

public sealed class MfaChallengeResponse
{
    public Guid ChallengeId { get; init; }
    public string Channel { get; init; } = default!;
    public string[] AvailableChannels { get; init; } = Array.Empty<string>();
    public DateTimeOffset ExpiresAt { get; init; }
}
