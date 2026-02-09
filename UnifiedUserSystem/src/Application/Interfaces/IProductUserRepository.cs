namespace UnifiedUserSystem.src.Application.Interfaces
{
    public interface IProductUserRepository
    {
        Task<bool> HasAccessAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
        Task AddAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default);
    }
}
