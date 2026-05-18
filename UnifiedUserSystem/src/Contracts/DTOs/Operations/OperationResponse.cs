namespace UnifiedUserSystem.src.Contracts.DTOs.Operations
{
    public class OperationResponse
    {
        public Guid Id { get; set; }
        public string Key { get; set; } = default!;
        public string Title { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
