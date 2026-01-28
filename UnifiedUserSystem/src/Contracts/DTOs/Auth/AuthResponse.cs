namespace UnifiedUserSystem.src.Contracts.DTOs
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
