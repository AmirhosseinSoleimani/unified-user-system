using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.src.Application.Services
{
    public class OperationService : IOperationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;

        public OperationService(IUnitOfWork uow, IClock clock, ICurrentUser currentUser) 
        {
            _uow = uow;
            _clock = clock;
            _currentUser = currentUser;
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

            var op = Operation.Create(key, title, _clock.Utcnow, _currentUser.UserId);

            _uow.Operations.Add(op);
            await _uow.SaveChangesAsync(ct);

            return op;
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

            var normalizedKey = Operation.NormalizeKey(key);

            var exists = await _uow.Operations.FindByKeyAsync(normalizedKey, ct);
            if (exists is not null && exists.Id != operationId)
                throw new InvalidOperationException("Operation key already exists.");

            op.ChangeKey(key, _clock.Utcnow, _currentUser.UserId);
            op.RenameTitle(title, _clock.Utcnow, _currentUser.UserId);

            await _uow.SaveChangesAsync(ct);

            return op;
        }

        public async Task DeleteOperationAsync(Guid operationId, CancellationToken ct = default)
        {
            Guard.True(operationId != Guid.Empty, "OperationId is invalid.");

            var op = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new KeyNotFoundException("Operation not found.");

            var hasAssignedRoles = await _uow.Operations.HasAssignedRolesAsync(operationId, ct);
            if (hasAssignedRoles)
                throw new InvalidOperationException("Operation is assigned to roles and cannot be deleted.");

            op.Delete(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task ActivateOperatioAsync(Guid operationId, CancellationToken ct = default)
        {
            await ActivateOperationAsync(operationId, ct);
        }

        public async Task ActivateOperationAsync(Guid operationId, CancellationToken ct = default)
        {
            var op = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new InvalidOperationException("Operation not found.");

            op.Active(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task ChangeOperationKeyAsync(Guid operationId, string newKey, CancellationToken ct = default)
        {
            var newKeyLower = Operation.NormalizeKey(newKey);

            var exists = await _uow.Operations.FindByKeyAsync(newKeyLower, ct);
            if (exists is not null && exists.Id != operationId)
                throw new InvalidOperationException("Operation key already exists.");

            var op = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new InvalidOperationException("Operation not found.");

            op.ChangeKey(newKey, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task DeactivateOperationAsync(Guid operationId, CancellationToken ct = default)
        {
            var op = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new InvalidOperationException("Operation not found.");

            op.Deactive(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task RenameOperationTitleAsync(Guid operationId, string newTitle, CancellationToken ct = default)
        {
            var op = await _uow.Operations.FindByIdAsync(operationId, ct)
                ?? throw new InvalidOperationException("Operation not found.");

            op.RenameTitle(newTitle, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
