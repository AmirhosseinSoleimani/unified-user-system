namespace UnifiedUserSystem.src.Contracts.DTOs.Roles
{
    public class ReplaceRoleOperationsRequest
    {
        public IReadOnlyCollection<Guid> OperationIds { get; set; } = Array.Empty<Guid>();
    }
}
