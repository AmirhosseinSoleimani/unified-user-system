using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.Contracts.DTOs;

namespace UnifiedUserSystem.src.Business.validators
{
    public class UserBusiness : IUserBusiness
    {
        public void ValidateLogin(LoginRequest req)
        {
            if (req is null) throw new ArgumentException("Request is null.");
            if (string.IsNullOrWhiteSpace(req.EmailOrUsername)) throw new ArgumentException("EmailOrUsername is required.");
            if (string.IsNullOrWhiteSpace(req.Password)) throw new ArgumentException("Password is required.");
        }

        public void ValidateRegister(RegisterRequest req)
        {
            if (req is null) throw new ArgumentException("Request is null.");
            if (string.IsNullOrWhiteSpace(req.Email)) throw new ArgumentException("Email is required.");
            if (string.IsNullOrWhiteSpace(req.Username)) throw new ArgumentException("Username is required.");
            if (string.IsNullOrWhiteSpace(req.FullName)) throw new ArgumentException("FullName is required.");
            if (string.IsNullOrWhiteSpace(req.Password)) throw new ArgumentException("Password is required.");
            if(req.Password.Length < 0)
            throw new ArgumentException("Password must be at least 8 characters.");
        }
    }
}
