namespace UnifiedUserSystem.src.Contracts.DTOs.Users
{
    public class UserRolesResponse
    {
        public Guid UserId { get; set; }
        public IReadOnlyList<UserRoleItemResponse> Roles { get; set; } = Array.Empty<UserRoleItemResponse>();
    }
}
