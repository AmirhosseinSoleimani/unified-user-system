using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Services.Security;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.Application.Services.Authorization
{
    public class OperationService : IOperationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;
        private readonly IPermissionCacheInvalidator _permissionCacheInvalidator;

        public OperationService(
            IUnitOfWork uow,
            IClock clock,
            ICurrentUser currentUser,
            IPermissionCacheInvalidator? permissionCacheInvalidator = null)
        {
            _uow = uow;
            _clock = clock;
            _currentUser = currentUser;
            _permissionCacheInvalidator = permissionCacheInvalidator ?? NullPermissionCacheInvalidator.Instance;
        }

        public async Task<IReadOnlyList<Operation>> ListOperationsAsync(CancellationToken ct = default)
        {
            return await _uow.Operations.ListAsync(ct);
        }

        public async Task<Operation?> GetOperationByIdAsync(Guid operationId, CancellationToken ct = default)
        {
            Guard.True(operationId != Guid.Empty, "OperationId is invalid.");

            return await _uow.Operations.FindByIdAsync(operationId, ct);
        }

        public async Task<Operation> CreateOperationAsync(string key, string title, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new DomainException("Operation key is required.");

            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Operation title is required.");

            var normalizedKey = Operation.NormalizeKey(key);

            if (string.IsNullOrWhiteSpace(normalizedKey))
                throw new DomainException("Operation key is required.");

            var exists = await _uow.Operations.FindByKeyAsync(normalizedKey, ct);
            if (exists is not null)
                throw new InvalidOperationException("Operation key already exists.");

            var operation = Operation.Create(key, title, _clock.Utcnow, _currentUser.UserId);

            _uow.Operations.Add(operation);
            await _uow.SaveChangesAsync(ct);

            await _permissionCacheInvalidator.InvalidateForOperationAsync(operation.Key, ct);
            return operation;
        }

        public async Task<Operation> UpdateOperationAsync(Guid operationId, string key, string title, CancellationToken ct = default)
        {
            Guard.True(operationId != Guid.Empty, "OperationId is invalid.");

            if (string.IsNullOrWhiteSpace(key))
                throw new DomainException("Operation key is required.");

            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Operation title is required.");

            var op = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new KeyNotFoundException("Operation not found.");

            var oldKey = op.Key;

            var normalizedKey = Operation.NormalizeKey(key);

            var exists = await _uow.Operations.FindByKeyAsync(normalizedKey, ct);
            if (exists is not null && exists.Id != operationId)
                throw new InvalidOperationException("Operation key already exists.");

            op.ChangeKey(key, _clock.Utcnow, _currentUser.UserId);
            op.RenameTitle(title, _clock.Utcnow, _currentUser.UserId);

            await _uow.SaveChangesAsync(ct);
            await _permissionCacheInvalidator.InvalidateForOperationAsync(oldKey, ct);
            await _permissionCacheInvalidator.InvalidateForOperationAsync(op.Key, ct);
            return op;
        }

        public async Task DeleteOperationAsync(Guid operationId, CancellationToken ct = default)
        {
            Guard.True(operationId != Guid.Empty, "OperationId is invalid.");

            var op = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new KeyNotFoundException("Operation not found.");
            var operationKey = op.Key;

            var hasAssignedRoles = await _uow.Operations.HasAssignedRolesAsync(operationId, ct);
            if (hasAssignedRoles)
                throw new InvalidOperationException("Operation is assigned to roles and cannot be deleted.");

            op.Delete(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
            await _permissionCacheInvalidator.InvalidateForOperationAsync(operationKey, ct);
        }

        public async Task ActivateOperationAsync(Guid operationId, CancellationToken ct = default)
        {
            var operation = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new InvalidOperationException("Operation not found.");

            operation.Activate(_clock.Utcnow, _currentUser.UserId);

            await _uow.SaveChangesAsync(ct);
            await _permissionCacheInvalidator.InvalidateForOperationAsync(operation.Key, ct);
        }

        public async Task ChangeOperationKeyAsync(Guid operationId, string newKey, CancellationToken ct = default)
        {
            var newKeyLower = Operation.NormalizeKey(newKey);

            var exists = await _uow.Operations.FindByKeyAsync(newKeyLower, ct);
            if (exists is not null && exists.Id != operationId)
                throw new InvalidOperationException("Operation key already exists.");

            var operation = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new InvalidOperationException("Operation not found.");

            var oldKey = operation.Key;

            operation.ChangeKey(newKey, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);

            await _permissionCacheInvalidator.InvalidateForOperationAsync(oldKey, ct);
            await _permissionCacheInvalidator.InvalidateForOperationAsync(operation.Key, ct);
        }

        public async Task DeactivateOperationAsync(Guid operationId, CancellationToken ct = default)
        {
            var operation = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new InvalidOperationException("Operation not found.");

            operation.Deactivate(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);

            await _permissionCacheInvalidator.InvalidateForOperationAsync(operation.Key, ct);
        }

        public async Task RenameOperationTitleAsync(Guid operationId, string newTitle, CancellationToken ct = default)
        {
            var operation = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new InvalidOperationException("Operation not found.");

            operation.RenameTitle(newTitle, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
