using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Users;
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
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Role name is required.");

            var normalizedName = Role.NormalizeName(name);

            if (string.IsNullOrWhiteSpace(normalizedName))
                throw new DomainException("Role name is required.");

            var existingByName = await _uow.Roles.FindByNameAsync(normalizedName, ct);
            if (existingByName is not null)
                throw new InvalidOperationException("Role name already exists.");

            var baseKey = Role.NormalizeKey(normalizedName);
            if (string.IsNullOrWhiteSpace(baseKey))
                throw new DomainException("Role name is required.");

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

        public Task<IReadOnlyList<Role>> ListRolesAsync(CancellationToken ct = default)
        {
            return _uow.Roles.ListAsync(ct);
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId, CancellationToken ct = default)
        {
            Guard.True(roleId > 0, "RoleId is invalid.");

            return await _uow.Roles.FindByIdAsync(roleId, ct);
        }

        public async Task<Role> UpdateRoleAsync(int roleId, string name, CancellationToken ct = default)
        {
            Guard.True(roleId > 0, "RoleId is invalid.");

            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Role name is required.");

            var role = await _uow.Roles.FindByIdAsync(roleId, ct)
                ?? throw new KeyNotFoundException("Role not found.");

            var normalizedName = Role.NormalizeName(name);
            var existingByName = await _uow.Roles.FindByNameAsync(normalizedName, ct);

            if (existingByName is not null && existingByName.Id != roleId)
                throw new InvalidOperationException("Role name already exists.");

            role.Rename(normalizedName, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);

            return role;
        }

        public async Task DeleteRoleAsync(int roleId, CancellationToken ct = default)
        {
            Guard.True(roleId > 0, "RoleId is invalid.");

            var role = await _uow.Roles.FindByIdAsync(roleId, ct)
                ?? throw new KeyNotFoundException("Role not found.");

            var hasAssignedUsers = await _uow.Roles.HasAssignedUsersAsync(roleId, ct);
            if (hasAssignedUsers)
                throw new InvalidOperationException("Role is assigned to users and cannot be deleted.");

            role.Delete(_clock.Utcnow, _currentUser.UserId);
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

        public async Task<UserRolesResponse> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await GetUserWithRolesOrThrowAsync(userId, ct);

            return ToUserRolesResponse(user);
        }

        public async Task<UserRolesResponse> AssignRoleToUserAsync(Guid userId, int roleId, CancellationToken ct = default)
        {
            Guard.True(roleId > 0, "RoleId is invalid.");

            var user = await GetUserWithRolesOrThrowAsync(userId, ct);
            var role = await GetActiveRoleOrThrowAsync(roleId, ct);

            user.AssignRole(role.Id, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);

            return ToUserRolesResponse(user);
        }

        public async Task<UserRolesResponse> RemoveRoleFromUserAsync(Guid userId, int roleId, CancellationToken ct = default)
        {
            Guard.True(roleId > 0, "RoleId is invalid.");

            var user = await GetUserWithRolesOrThrowAsync(userId, ct);

            var role = await _uow.Roles.FindByIdAsync(roleId, ct)
                ?? throw new KeyNotFoundException("Role not found.");

            user.RemoveRole(role.Id, _clock.Utcnow, _currentUser.UserId);
            await _uow.SaveChangesAsync(ct);

            return ToUserRolesResponse(user);
        }

        public async Task<UserRolesResponse> ReplaceUserRolesAsync(Guid userId, IReadOnlyCollection<int> roleIds, CancellationToken ct = default)
        {
            if (roleIds is null)
                throw new DomainException("RoleIds is required.");

            var duplicateRoleIds = roleIds
                .GroupBy(x => x)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToArray();

            if (duplicateRoleIds.Length > 0)
                throw new DomainException("Duplicate role ids are not allowed.");

            if (roleIds.Any(x => x <= 0))
                throw new DomainException("RoleId is invalid.");

            var user = await GetUserWithRolesOrThrowAsync(userId, ct);
            var requestedRoleIds = roleIds.ToHashSet();

            foreach (var roleId in requestedRoleIds)
            {
                await GetActiveRoleOrThrowAsync(roleId, ct);
            }

            var currentRoleIds = user.UserRoles
                .Select(x => x.RoleId)
                .ToArray();

            foreach (var currentRoleId in currentRoleIds)
            {
                if (!requestedRoleIds.Contains(currentRoleId))
                    user.RemoveRole(currentRoleId, _clock.Utcnow, _currentUser.UserId);
            }

            foreach (var roleId in requestedRoleIds)
            {
                user.AssignRole(roleId, _clock.Utcnow, _currentUser.UserId);
            }

            await _uow.SaveChangesAsync(ct);

            return ToUserRolesResponse(user);
        }

        private async Task<User> GetUserWithRolesOrThrowAsync(Guid userId, CancellationToken ct)
        {
            Guard.True(userId != Guid.Empty, "UserId is invalid.");

            return await _uow.Users.FindByIdWithRolesAsync(userId, ct)
                ?? throw new KeyNotFoundException("User not found.");
        }

        private async Task<Role> GetActiveRoleOrThrowAsync(int roleId, CancellationToken ct)
        {
            var role = await _uow.Roles.FindByIdAsync(roleId, ct)
                ?? throw new KeyNotFoundException("Role not found.");

            if (!role.IsActive)
                throw new InvalidOperationException("Role is not active.");

            return role;
        }

        private static UserRolesResponse ToUserRolesResponse(User user)
        {
            var roles = user.UserRoles
                .Where(x => x.Role != null)
                .OrderBy(x => x.Role.Name)
                .Select(x => new UserRoleItemResponse
                {
                    RoleId = x.RoleId,
                    Name = x.Role.Name,
                    IsActive = x.Role.IsActive
                })
                .ToArray();

            return new UserRolesResponse
            {
                UserId = user.Id,
                Roles = roles
            };
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
