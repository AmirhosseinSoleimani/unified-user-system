using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordHasher _hasher;
        private readonly IUserBusiness _business;
        private readonly IJwtTokenService _jwt;
        private readonly IClock _clock;
        public AuthService(
            IUnitOfWork uow,
            IPasswordHasher hasher,
            IUserBusiness business,
            IJwtTokenService jwt,
            IClock clock
            )
        {
            _uow = uow;
            _hasher = hasher;
            _business = business;
            _jwt = jwt;
            _clock = clock;
        }
        public async Task<AuthResponse> RegisterAsync(RegisterRequest req) 
        {
            _business.ValidateRegister(req);

            var email = User.NormalizeEmail(req.Email);
            var username = User.NormalizeUsername(req.Username);
            var fullName = User.NormalizeFullname(req.FullName);

            if (await _uow.Users.EmailExistsAsync(email))
                throw new InvalidOperationException("Email already exists.");

            if (await _uow.Users.UsernameExistsAsync(username))
                throw new InvalidOperationException("Username already exists.");

            var defaultRoleId = (int)AppRole.User;
            var role = await _uow.Roles.FindByIdAsync(defaultRoleId)
                ?? throw new InvalidOperationException("Default role not found. Seed roles first.");

            var hash = _hasher.Hash(req.Password);
            var now = _clock.Utcnow;

            var user = User.CreateNew(email, username, fullName, hash, now, actorUserId: null);
            user.AssignRole(roleId: 1, now, actorUserId: user.Id);

            _uow.Users.Add(user);
            await _uow.SaveChangesAsync();

            var roles = user.UserRoles
                .Select(x => x.Role?.Name ?? "user")
                .Distinct()
                .ToArray();

            var accessToken = _jwt.CreateAccessToken(user);

            return new AuthResponse(user.Id, user.Email, user.Username, user.Fullname, roles, accessToken);

        }
        public async Task<AuthResponse?> LoginAsync(LoginRequest req) 
        {
            _business.ValidateLogin(req);

            var keyLower = req.EmailOrUsername.Trim().ToLowerInvariant();
            var user = await _uow.Users.FindEmailOrUsernameAsync(keyLower);

            if (user is null) return null;
            if(!user.IsActive) return null;

            if (!_hasher.Verify(req.Password, user.PasswordHash))
                return null;

            var roles = user.UserRoles.Select(x => x.Role.Name).Distinct().ToArray();
            var accessToken = _jwt.CreateAccessToken(user);

            return new AuthResponse(user.Id, user.Email, user.Username, user.Fullname, roles, accessToken);
        }
    }
}
