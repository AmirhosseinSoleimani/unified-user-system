namespace UnifiedUserSystem.src.Contracts.DTOs
{
    public class LoginRequest
    {
        public string EmailOrUsername { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
}
