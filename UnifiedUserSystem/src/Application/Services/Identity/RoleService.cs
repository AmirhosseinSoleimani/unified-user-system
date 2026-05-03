using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.src.Application.Services.Identity
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

        public async Task<Role> CreateRoleAsync(string name, CancellationToken ct = default)
        {
            var normalizedName = Role.NormalizeName(name);

            var existingByName = await _uow.Roles.FindByNameAsync(normalizedName, ct);
            if (existingByName is not null)
                throw new InvalidOperationException("Role name already exists.");

            var baseKey = Role.NormalizeKey(normalizedName);
            if (string.IsNullOrWhiteSpace(baseKey))
                throw new InvalidOperationException("Role key cannot be generated from name.");

            var key = await GenerateUniqueKeyAsync(baseKey, ct);

            var role = Role.Create(key, normalizedName, _clock.Utcnow, _currentUser.UserId);

            _uow.Roles.Add(role);
            await _uow.SaveChangesAsync(ct);

            return role;
        }

        public async Task DeactivateRoleAsync(int roleId, CancellationToken ct = default)
        {
            var role = await _uow.Roles.FindByIdAsync(roleId, ct)
                ?? throw new InvalidOperationException("Role not found.");

            role.Deactivate(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task ActivateRoleAsync(int roleId, CancellationToken ct = default)
        {
            var role = await _uow.Roles.FindByIdAsync(roleId, ct)
                ?? throw new InvalidOperationException("Role not found.");

            role.Activate(_clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task AssignRoleToUserAsync(Guid userId, int roleId, CancellationToken ct = default)
        {
            Guard.True(userId != Guid.Empty, "UserId not found.");

            var user = await _uow.Users.FindByIdAsync(userId, ct)
                ?? throw new InvalidOperationException("User not found.");

            var role = await _uow.Roles.FindByIdAsync(roleId, ct)
                ?? throw new InvalidOperationException("Role not found.");

            if (!role.IsActive) throw new InvalidOperationException("Role is not active.");

            user.AssignRole(roleId, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, int roleId, CancellationToken ct = default)
        {
            Guard.True(userId != Guid.Empty, "UserId is invalid.");

            var user = await _uow.Users.FindByIdAsync(userId, ct)
                ?? throw new InvalidOperationException("User not found.");

            user.RemoveRole(roleId, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }

        public async Task RenameRoleAsync(int roleId, string newName, CancellationToken ct = default)
        {
            var role = await _uow.Roles.FindByIdAsync(roleId, ct)
                ?? throw new InvalidOperationException("Role not found.");

            var normalizedName = Role.NormalizeName(newName);

            var exists = await _uow.Roles.FindByNameAsync(normalizedName, ct);
            if (exists is not null && exists.Id != roleId)
                throw new InvalidOperationException("Role name already exists.");

            role.Rename(newName, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);
        }

        private async Task<string> GenerateUniqueKeyAsync(string baseKey, CancellationToken ct)
        {
            var key = baseKey;
            var suffix = 2;

            while (await _uow.Roles.ExistsByKeyAsync(key, ct)) 
            {
                key = $"{baseKey}-{suffix}";
                suffix++;

                if (key.Length > Role.KeyMaxLength)
                    throw new InvalidOperationException("Generated role key is too long.");
            }
            return key;
        }
    }
}
