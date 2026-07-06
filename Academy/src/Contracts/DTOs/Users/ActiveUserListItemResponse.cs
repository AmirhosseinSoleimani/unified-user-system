namespace UnifiedUserSystem.src.Contracts.DTOs.Users
{
    public class ActiveUserListItemResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Fullname { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
