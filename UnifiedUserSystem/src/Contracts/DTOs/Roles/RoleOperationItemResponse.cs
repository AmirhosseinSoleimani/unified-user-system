namespace UnifiedUserSystem.src.Contracts.DTOs.Roles
{
    public class RoleOperationItemResponse
    {
        public Guid OperationId { get; set; }
        public string Key { get; set; } = default!;
        public string Title { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
