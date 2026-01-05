using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Application
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _hasher;
        private readonly IUserBusiness _business;
        private readonly IJwtTokenService _jwt;
        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher hasher, IUserBusiness business, IJwtTokenService jwt)
        {
            _unitOfWork = unitOfWork;
            _hasher = hasher;
            _business = business;
            _jwt = jwt;
        }
        public async Task<AuthResponse> RegisterAsync(RegisterRequest req) 
        {
            _business.ValidateRegister(req);

            var email = req.Email.Trim().ToLowerInvariant();
            var username = req.Username.Trim();
            var fullName = req.FullName.Trim();

            if (await _unitOfWork.Users.EmailExistsAsync(email))
                throw new InvalidOperationException("Email already exists.");

            if (await _unitOfWork.Users.UsernameExistsAsync(username))
                throw new InvalidOperationException("Username already exists.");

            var hash = _hasher.Hash(req.Password);
            var now = DateTimeOffset.UtcNow;

            var user = User.CreateNew(email, username, fullName, hash, now, actorUserId: null);
            _unitOfWork.Users.Add(user);
            await _unitOfWork.SaveChangesAsync();
            var accessToken = _jwt.CreateAccessToken(user);
            return new AuthResponse(user.Id, user.Email, user.Username, user.Fullname, user.Role, accessToken);

        }
        public async Task<AuthResponse?> LoginAsync(LoginRequest req) 
        {
            _business.ValidateLogin(req);
            var keyLower = req.EmailOrUsername.Trim().ToLowerInvariant();

            var user = await _unitOfWork.Users.FindEmailOrUsernameAsync(keyLower);
            if (user is null) return null;

            if (!_hasher.Verify(req.Password, user.PasswordHash))
                return null;

            var accessToken = _jwt.CreateAccessToken(user);

            return new AuthResponse(user.Id, user.Email, user.Username, user.Fullname, user.Role, accessToken);
        }
    }
}
