namespace UnifiedUserSystem.src.Contracts.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;

        [Obsolete("Use FirstName and LastName instead.")]
        public string FullName { get; set; } = default!;

        public string Password { get; set; } = default!;
    }
}
