using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
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

        public async Task ActivateOperatioAsync(Guid operationId, CancellationToken ct = default)
        {
            var op = await _uow.Operations.FindByIdAsync(operationId)
                ?? throw new InvalidOperationException("Operation not found.");

            op.Active(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync();
        }

        public async Task ChangeOperationKeyAsync(Guid operationId, string newKey, CancellationToken ct = default)
        {
            var newKeyLower = Operation.NormalizeKey(newKey);
            var exists = await _uow.Operations.FindByKeyAsync(newKeyLower);
            if (exists is not null && exists.Id != operationId)
                throw new InvalidOperationException("Operation key already exists.");

            var op = await _uow.Operations.FindByIdAsync(operationId)
                ?? throw new InvalidOperationException("Operation not found.");

            op.ChangeKey(newKey, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync();

        }
        public async Task<Operation> CreateOperationAsync(string key, string title, CancellationToken ct = default)
        {
            var keyLower = Operation.NormalizeKey(key);
            var exists = await _uow.Operations.FindByKeyAsync(keyLower);
            if (exists is not null)
                throw new InvalidOperationException("Operation key already exists.");

            var op = Operation.Create(key, title, _clock.Utcnow, _currentUser.UserId);
            _uow.Operations.Add(op);
            await _uow.SaveChangesAsync();
            return op;
        }

        public async Task DeactivateOperationAsync(Guid operationId, CancellationToken ct = default)
        {
            var op = await _uow.Operations.FindByIdAsync(operationId)
                ?? throw new InvalidOperationException("Operation not found.");

            op.Deactive(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync();
        }

        public async Task RenameOperationTitleAsync(Guid operationId, string newTitle, CancellationToken ct = default)
        {
            var op = await _uow.Operations.FindByIdAsync(operationId)
                ?? throw new InvalidOperationException("Operation not found.");

            op.RenameTitle(newTitle, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync();
            
        }
    }
}
