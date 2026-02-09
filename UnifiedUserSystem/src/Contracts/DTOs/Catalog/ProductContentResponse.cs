namespace UnifiedUserSystem.src.Contracts.DTOs.Catalog
{
    public sealed record ProductContentResponse(
        Guid Id,
        string Title,
        string Content
    );
}
