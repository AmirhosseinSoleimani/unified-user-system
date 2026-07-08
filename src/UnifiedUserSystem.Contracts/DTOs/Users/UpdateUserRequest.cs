namespace UnifiedUserSystem.src.Contracts.DTOs.Users
{
    public class UpdateUserRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }

        [Obsolete("Use FirstName and LastName instead.")]
        public string? Fullname { get; set; }

        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
