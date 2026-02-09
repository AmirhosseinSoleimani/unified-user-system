namespace UnifiedUserSystem.src.Contracts.DTOs.Product
{
    public class UpdateProductRequest
    {
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string Content { get; set; } = default!;
        public decimal Price { get; set; }
    }
}
