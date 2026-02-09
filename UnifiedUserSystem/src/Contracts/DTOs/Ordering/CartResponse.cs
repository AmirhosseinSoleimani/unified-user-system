namespace UnifiedUserSystem.src.Contracts.DTOs.Order
{
    public sealed record CartItemResponse(
        Guid ProductId,
        decimal UnitPrice
    );

    public sealed record CartResponse(
        Guid OrderId,
        bool IsConfirm,
        decimal TotalPrice,
        CartItemResponse[] Items
    );
}
