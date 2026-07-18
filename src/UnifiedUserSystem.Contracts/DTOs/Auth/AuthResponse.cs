using System.Text.Json.Serialization;

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
    [property: JsonIgnore] string RefreshToken,
    [property: JsonIgnore] DateTimeOffset RefreshTokenExpiresAtUt
)
{
    public string? PreferredLocale { get; init; }
}
