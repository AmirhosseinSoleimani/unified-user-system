namespace UnifiedUserSystem.src.Contracts.DTOs.Catalog
{
    public sealed record ProductListItemResponse(
        Guid Id,
        string Title,
        string Description,
        decimal Price,
        bool IsActive
    );
}
