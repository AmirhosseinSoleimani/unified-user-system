
namespace UnifiedUserSystem.src.Contracts.DTOs.Auth;

public sealed class LoginResponse
{
    public bool RequiresMfa { get; init; }
    public AuthResponse? Tokens { get; init; }
    public MfaChallengeResponse? MfaChallenge { get; init; }

    public static LoginResponse Authenticated(AuthResponse tokens)
        => new()
        {
            RequiresMfa = false,
            Tokens = tokens
        };

    public static LoginResponse Challenge(MfaChallengeResponse challenge)
        => new()
        {
            RequiresMfa = true,
            MfaChallenge = challenge
        };
}
