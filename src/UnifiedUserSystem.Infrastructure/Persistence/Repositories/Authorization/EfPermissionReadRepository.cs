using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Security;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Infrastructure.Persistence.Repositories.Authorization
{
    public sealed class EfPermissionReadRepository : IPermissionReadRepository
    {
        private readonly AppDbContext _db;

        public EfPermissionReadRepository(AppDbContext db)
        {
            _db = db;
        }

        public Task<bool> UserHasOperationAsync(
            Guid userId,
            string operationKey,
            CancellationToken cancellationToken = default)
        {
            operationKey = OperationPolicyNames.NormalizeOperationKey(operationKey);

            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(operationKey))
                return Task.FromResult(false);

            return _db.UserRoles.AnyAsync(
                userRole =>
                    userRole.UserId == userId &&
                    userRole.User.IsActive &&
                    userRole.Role.IsActive &&
                    userRole.Role.RoleOperations.Any(roleOperation =>
                        roleOperation.Operation.IsActive &&
                        roleOperation.Operation.Key == operationKey),
                cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> ListUserIdsInRoleAsync(
            int roleId,
            CancellationToken cancellationToken = default)
        {
            if (roleId <= 0)
                return Array.Empty<Guid>();

            return await _db.UserRoles
                .AsNoTracking()
                .Where(userRole => userRole.RoleId == roleId)
                .Select(userRole => userRole.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
    }
}