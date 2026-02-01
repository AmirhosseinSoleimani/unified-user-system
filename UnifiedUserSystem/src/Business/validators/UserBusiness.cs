using System.Text.RegularExpressions;
using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Security;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.Business.validators
{
    public class UserBusiness : IUserBusiness
    {
        private readonly IPasswordPolicy _passwordPolicy;

        public UserBusiness(IPasswordPolicy passwordPolicy)
        {
            _passwordPolicy = passwordPolicy;
        }

        public void ValidateLogin(LoginRequest req)
        {
            if (req is null) throw new DomainException("Request is null.");

            var key = Guard.NotEmpty(req.EmailOrUsername, nameof(req.EmailOrUsername));
            var pwd = Guard.NotEmpty(req.Password, nameof(req.Password));

            if (pwd.Length < 1) throw new DomainException("Password is required");
        }

        public void ValidateRegister(RegisterRequest req)
        {
            if (req is null) throw new DomainException("Request is null.");

            var email = Guard.NotEmpty(req.Email, nameof(req.Email));
            var username = Guard.NotEmpty(req.Username, nameof(req.Username));
            var fullName = Guard.NotEmpty(req.FullName, nameof(req.FullName));
            var password = Guard.NotEmpty(req.Password, nameof(req.Password));

            email = User.NormalizeEmail(email);
            username = User.NormalizeUsername(username);
            fullName = User.NormalizeFullname(fullName);

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new DomainException("Email format is invalid.");

            if (username.Length < 3)
                throw new DomainException("Username must be at least 3 characters.");

            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9._-]+$"))
                throw new DomainException("Username contains invalid characters.");

            _passwordPolicy.Validate(password);
        }
    }
}
