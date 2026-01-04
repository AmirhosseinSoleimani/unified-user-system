namespace UnifiedUserSystem.src.UnifiedUserSystem.Contracts.DTOs
{
    public record RegisterRequest(string Email, string Username, string FullName, string Password);
    public record LoginRequest(string EmailOrUsername, string Password);
    public record AuthResponse(Guid Id,string Email,string Username,string FullName,string Role
    );
}
