namespace UnifiedUserSystem.src.Contracts.DTOs.Users
{
    public class UpdateUserRequest
    {
        public string? Fullname { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
