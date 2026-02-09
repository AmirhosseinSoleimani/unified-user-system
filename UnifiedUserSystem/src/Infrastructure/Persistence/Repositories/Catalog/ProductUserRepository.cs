using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Catalog.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories
{
    public class ProductUserRepository : IProductUserRepository
    {
        private readonly AppDbContext _db;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;

        public ProductUserRepository(AppDbContext db, IClock clock, ICurrentUser currentUser)
        {
            _db = db;
            _clock = clock;
            _currentUser = currentUser;
        }

        public async Task AddAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
        {
            var exists = await HasAccessAsync(userId, productId, cancellationToken);
            if (exists) return;

            var link = ProductUser.Grant(userId, productId, _clock.Utcnow, _currentUser.UserId);
            _db.ProductUsers.Add(link);
        }

        public Task<bool> HasAccessAsync(Guid userId, Guid productId, CancellationToken cancellationToken = default)
        {
            return _db.ProductUsers.AnyAsync(x => x.UserId == userId && x.ProductId == productId, cancellationToken);
        }
    }
}
