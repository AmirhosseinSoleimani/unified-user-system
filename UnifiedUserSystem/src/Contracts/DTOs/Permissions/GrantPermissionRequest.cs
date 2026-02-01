namespace UnifiedUserSystem.src.Contracts.DTOs.Permissions
{
    public class GrantPermissionRequest
    {
        public int RoleId { get; set; }
        public Guid OperationId { get; set; }
    }
}
