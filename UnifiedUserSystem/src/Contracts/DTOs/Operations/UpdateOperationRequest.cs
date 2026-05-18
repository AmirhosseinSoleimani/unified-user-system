namespace UnifiedUserSystem.src.Contracts.DTOs.Operations
{
    public class UpdateOperationRequest
    {
        public string Key { get; set; } = default!;
        public string Title { get; set; } = default!;
    }
}
