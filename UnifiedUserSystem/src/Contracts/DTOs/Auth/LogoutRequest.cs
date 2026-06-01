namespace UnifiedUserSystem.src.Contracts.DTOs.Auth
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = default!;
    }
}
