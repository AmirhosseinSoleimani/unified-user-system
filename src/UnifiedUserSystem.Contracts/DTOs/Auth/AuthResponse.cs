namespace UnifiedUserSystem.src.Contracts.DTOs.Auth;

public record AuthResponse
(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string Fullname,
    string[] Roles,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc
)
{
    public string? PreferredLocale { get; init; }
}
