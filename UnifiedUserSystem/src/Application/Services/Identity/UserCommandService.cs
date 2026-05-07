using Microsoft.AspNetCore.Identity;
using UnifiedUserSystem.src.Application.Interfaces;
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

        public UserCommandService(
            IUnitOfWork unitOfWork,
            IClock clock,
            ICurrentUser currentUser,
            IPasswordHasher passwordHasher,
            IPasswordPolicy passwordPolicy
            )
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
            _currentUser = currentUser;
            _passwordHasher = passwordHasher;
            _passwordPolicy = passwordPolicy;
        }

        public async Task DeactivateUserAsync(Guid id, CancellationToken ct = default)
        {
            var user = await _unitOfWork.Users.FindByIdAsync(id, ct);

            if (user is null)
                throw new KeyNotFoundException("User not found.");

            user.Deactive(_clock.Utcnow, _currentUser.UserId);
            await _unitOfWork.SaveChangesAsync(ct);

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

            await _unitOfWork.SaveChangesAsync(ct);

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
