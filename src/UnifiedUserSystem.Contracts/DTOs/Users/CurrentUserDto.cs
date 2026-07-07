namespace UnifiedUserSystem.src.Contracts.DTOs.Users
{
    public class CurrentUserDto
    {
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string[] Roles { get; set; } = Array.Empty<string>();
        public bool IsAuthenticated => UserId.HasValue;
    }
}
