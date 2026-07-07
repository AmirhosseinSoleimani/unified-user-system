namespace UnifiedUserSystem.src.Contracts.DTOs.Users
{
    public class ReplaceUserRolesRequest
    {
        public IReadOnlyCollection<int> RoleIds { get; set; } = Array.Empty<int>();
    }
}
