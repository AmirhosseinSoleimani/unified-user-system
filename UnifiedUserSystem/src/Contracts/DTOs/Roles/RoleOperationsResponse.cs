namespace UnifiedUserSystem.src.Contracts.DTOs.Roles
{
    public class RoleOperationsResponse
    {
        public int RoleId { get; set; }
        public IReadOnlyList<RoleOperationItemResponse> Operations { get; set; } = Array.Empty<RoleOperationItemResponse>();
    }
}
