namespace UnifiedUserSystem.src.Contracts.DTOs.Permissions
{
    public class RevokePermissionRequest
    {
        public int RoleId { get; set; }
        public Guid OperationId { get; set; }
    }
}
