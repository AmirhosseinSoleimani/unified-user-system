namespace UnifiedUserSystem.src.Contracts.DTOs.Profile
{
    public class ProfileResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public bool IsActive { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
    }
}
