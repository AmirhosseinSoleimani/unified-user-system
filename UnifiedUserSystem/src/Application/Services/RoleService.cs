using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;

        public RoleService(IUnitOfWork uow, IClock clock, ICurrentUser currentUser)
        {
            _uow = uow;
            _clock = clock;
            _currentUser = currentUser;
        }

        public async Task ActivateRoleAsync(int roleId, CancellationToken ct = default)
        {
            var role = await _uow.Roles.FindByIdAsync(roleId)
                ?? throw new InvalidOperationException("Role not found.");
            role.Active(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync();
        }

        public async Task AssignRoleToUserAsync(Guid userId, int roleId, CancellationToken ct = default)
        {
            Guard.True(userId != Guid.Empty, "UserId not found.");
            var user = await _uow.Users.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            var role = await _uow.Users.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("Role not found.");

            if (!role.IsActive) throw new InvalidOperationException("Role is not active.");

            user.AssignRole(roleId, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync();
        }

        public async Task<Role> CreateRoleAsync(string name, CancellationToken ct = default)
        {
            var normalized = Role.NormalizeName(name);
            var exists = await _uow.Roles.FindByNameAsync(normalized);
            if (exists is not null)
                throw new InvalidOperationException("Rple name already exists.");
            var role = Role.Create(name, _clock.Utcnow, _currentUser.UserId);
            _uow.Roles.Add(role);
            await _uow.SaveChangesAsync();
            return role;
        }

        public async Task DeactivateRoleAsync(int roleId, CancellationToken ct = default)
        {
            var role = await _uow.Roles.FindByIdAsync(roleId)
                ?? throw new InvalidOperationException("Role not found.");
            role.Deactive(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, int roleId, CancellationToken ct = default)
        {
            Guard.True(userId != Guid.Empty, "UserId is invalid.");
            var user = await _uow.Users.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found.");

            user.RemoveRole(roleId, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync();
        }

        public async Task RenameRoleAsync(int roleId, string newName, CancellationToken ct = default)
        {
            var role = await _uow.Roles.FindByIdAsync(roleId)
                ?? throw new InvalidOperationException("Role not found.");
            var normalize = Role.NormalizeName(newName);
            var exists = await _uow.Roles.FindByNameAsync(normalize);
            if (exists is not null && exists.Id != roleId)
                throw new InvalidOperationException("Role name already exists.");

            role.Rename(newName, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync();
        }
    }
}
