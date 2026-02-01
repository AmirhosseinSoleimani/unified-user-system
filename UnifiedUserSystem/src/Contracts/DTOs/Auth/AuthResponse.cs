namespace UnifiedUserSystem.src.Contracts.DTOs.Auth
{
    public record AuthResponse
    (
        Guid Id,
        string Email,
        string Username,
        string Fullname,
        string[] Roles,
        string AccessToken
    );
}
