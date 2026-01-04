using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Contracts.DTOs;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Application
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _hasher;
        private readonly IUserBusiness _business;
        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher hasher, IUserBusiness business)
        {
            _unitOfWork = unitOfWork;
            _hasher = hasher;
            _business = business;
        }
        public async Task<AuthResponse> RegisterAsync(RegisterRequest req) 
        {
            var email = req.Email.Trim().ToLowerInvariant();
            var username = req.Username.Trim();
            var fullName = req.FullName.Trim();

            if (
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(req.FullName) ||
            string.IsNullOrWhiteSpace(req.Password)
            )
            {
                throw new ArgumentException("Invalid input.");
            }
            if (await _unitOfWork.Users.EmailExistsAsync(email))
                throw new InvalidOperationException("Email already exists.");

            if (await _unitOfWork.Users.UsernameExistsAsync(username))
                throw new InvalidOperationException("Username already exists.");

            var hash = _hasher.Hash(req.Password);
            var now = DateTimeOffset.UtcNow;

            var user = User.CreateNew(email, username, fullName, hash, now);
            _unitOfWork.Users.Add(user);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponse(user.Id, user.Email, user.Username, user.Fullname, user.Role);

        }
        public async Task<AuthResponse?> LoginAsync(LoginRequest req) 
        {
            var key = req.EmailOrUsername.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(req.Password)) 
            {
                throw new ArgumentException("Invalid input.");
            }
            var user = await _unitOfWork.Users.FindEmailOrUsernameAsync(key);
            if (user is null) return null;

            if (!_hasher.Verify(req.Password, user.PasswordHash))
                return null;

            return new AuthResponse(user.Id, user.Email, user.Username, user.Fullname, user.Role);
        }
    }
}
