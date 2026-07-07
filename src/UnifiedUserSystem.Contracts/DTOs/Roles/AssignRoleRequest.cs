namespace UnifiedUserSystem.src.Contracts.DTOs.Roles
{
    public class AssignRoleRequest
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
    }
}
