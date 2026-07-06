namespace UnifiedUserSystem.src.Contracts.DTOs.Auth
{
    public class LoginRequest
    {
        public string EmailOrUsername { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
