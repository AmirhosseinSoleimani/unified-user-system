using Microsoft.AspNetCore.Identity;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Auditing;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Contracts.DTOs.Profile;
using UnifiedUserSystem.src.Contracts.DTOs.Users;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;

namespace UnifiedUserSystem.src.Application.Services.Identity
{
    public class UserCommandService : IUserCommandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        private readonly ICurrentUser _currentUser;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IPasswordPolicy _passwordPolicy;
        private readonly IAuditLogWriter _auditLogWriter;

        public UserCommandService(
            IUnitOfWork unitOfWork,
            IClock clock,
            ICurrentUser currentUser,
            IPasswordHasher passwordHasher,
            IPasswordPolicy passwordPolicy,
            IAuditLogWriter auditLogWriter
            )
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
            _currentUser = currentUser;
            _passwordHasher = passwordHasher;
            _passwordPolicy = passwordPolicy;
            _auditLogWriter = auditLogWriter;
        }

        public async Task DeactivateUserAsync(Guid id, CancellationToken ct = default)
        {
            var user = await _unitOfWork.Users.FindByIdAsync(id, ct);

            if (user is null)
                throw new KeyNotFoundException("User not found.");

            var wasActive = user.IsActive;

            user.Deactive(_clock.Utcnow, _currentUser.UserId);
            await _unitOfWork.SaveChangesAsync(ct);

            if (wasActive && !user.IsActive)
            {
                await _auditLogWriter.WriteAsync(new WriteAuditLogRequest
                {
                    ActorUserId = _currentUser.UserId,
                    TargetUserId = user.Id,
                    EntityName = nameof(User),
                    EntityId = user.Id.ToString(),
                    Action = "UserDeactivated",
                    OldValues = new Dictionary<string, object?>
                    {
                        ["IsActive"] = true
                    },
                    NewValues = new Dictionary<string, object?>
                    {
                        ["IsActive"] = false
                    }
                }, ct);
            }

        }

        public async Task<ProfileResponse> UpdateUserAsync(Guid id, UpdateUserRequest req, CancellationToken ct = default)
        {
            if (req is null)
                throw new DomainException("Request is null.");

            var hasFullname = !string.IsNullOrWhiteSpace(req.Fullname);
            var hasUsername = !string.IsNullOrWhiteSpace(req.Username);
            var hasPassword = !string.IsNullOrWhiteSpace(req.Password);

            if (!hasFullname && !hasUsername && !hasPassword)
                throw new DomainException("At least one updatable field must be provided.");

            var user = await _unitOfWork.Users.FindByIdWithRolesAsync(id, ct);

            if (user is null)
                throw new KeyNotFoundException("User not found.");

            var now = _clock.Utcnow;
            var actorUserId = _currentUser.UserId;
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();
            var originalFullname = user.Fullname;
            var originalUsername = user.Username;

            if (hasFullname)
            {
                user.ChangeFullName(req.Fullname!, now, actorUserId);
            }

            if (hasUsername)
            {
                var normalizedUsername = User.NormalizeUsername(req.Username!);

                if (!string.Equals(user.Username, normalizedUsername, StringComparison.Ordinal))
                {
                    var usernameExists = await _unitOfWork.Users.UsernameExistsAsync(normalizedUsername);
                    if (usernameExists)
                        throw new InvalidOperationException("Username already exists.");
                }

                user.ChangeUsername(req.Username!, now, actorUserId);
            }

            if (hasPassword)
            {
                _passwordPolicy.Validate(req.Password!);
                var passwordHash = _passwordHasher.Hash(req.Password!);
                user.ChangePasswordHash(passwordHash, now, actorUserId);
            }

            if (!string.Equals(originalFullname, user.Fullname, StringComparison.Ordinal))
            {
                oldValues["Fullname"] = originalFullname;
                newValues["Fullname"] = user.Fullname;
            }

            if (!string.Equals(originalUsername, user.Username, StringComparison.Ordinal))
            {
                oldValues["Username"] = originalUsername;
                newValues["Username"] = user.Username;
            }

            if (hasPassword)
            {
                oldValues["PasswordChanged"] = false;
                newValues["PasswordChanged"] = true;
            }


            await _unitOfWork.SaveChangesAsync(ct);

            if (oldValues.Count > 0 || newValues.Count > 0)
            {
                await _auditLogWriter.WriteAsync(new WriteAuditLogRequest
                {
                    ActorUserId = actorUserId,
                    TargetUserId = user.Id,
                    EntityName = nameof(User),
                    EntityId = user.Id.ToString(),
                    Action = "UserUpdated",
                    OldValues = oldValues,
                    NewValues = newValues
                }, ct);
            }


            var roles = user.UserRoles
                .Where(x => x.Role != null)
                .Select(x => x.Role.Name)
                .Distinct()
                .ToArray();

            return new ProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Fullname = user.Fullname,
                IsActive = user.IsActive,
                Roles = roles
            };
        }
    }
}
