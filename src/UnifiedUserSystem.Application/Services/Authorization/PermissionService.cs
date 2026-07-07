using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Domain.Authorization.Entities;


namespace UnifiedUserSystem.Application.Services.Authorization
{
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;
        private readonly IPermissionCacheInvalidator _permissionCacheInvalidator;

        public PermissionService(
            IUnitOfWork uow,
            IClock clock,
            ICurrentUser currentUser,
            IPermissionCacheInvalidator permissionCacheInvalidator)
        {
            _uow = uow;
            _clock = clock;
            _currentUser = currentUser;
            _permissionCacheInvalidator = permissionCacheInvalidator;
        }
        public async Task GrantOperationToRoleAsync(int roleId, Guid operationId, CancellationToken ct = default)
        {
            var role = await _uow.Roles.FindByIdAsync(roleId)
                ?? throw new InvalidOperationException("Role not found.");

            if (!role.IsActive) throw new InvalidOperationException("Role is not active");

            var op = await _uow.Operations.FindByIdAsync(operationId)
                ?? throw new InvalidOperationException("Operation not found.");

            if (!op.IsActive) throw new InvalidOperationException("Operation is not active.");

            var exists = await _uow.RoleOperations.ExistsAsync(roleId, operationId);
            if (exists) return;

            var link = RoleOperation.Create(roleId, operationId, _clock.Utcnow, _currentUser.UserId);
            await _uow.RoleOperations.AddAsync(link, ct);

            await _uow.SaveChangesAsync(ct);
            await _permissionCacheInvalidator.InvalidateForRoleAsync(roleId, ct);
        }

        public async Task RevokeOperationFromRoleAsync(int roleId, Guid operationId, CancellationToken ct = default)
        {
            var link = await _uow.RoleOperations.FindAsync(roleId, operationId, ct);
            if (link is null) return;

            _uow.RoleOperations.Remove(link);
            await _uow.SaveChangesAsync(ct);
            await _permissionCacheInvalidator.InvalidateForRoleAsync(roleId, ct);
        }
    }
}
