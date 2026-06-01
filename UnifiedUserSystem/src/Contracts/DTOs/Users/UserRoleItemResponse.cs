namespace UnifiedUserSystem.src.Contracts.DTOs.Users
{
    public class UserRoleItemResponse
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
