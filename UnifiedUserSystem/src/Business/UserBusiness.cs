using UnifiedUserSystem.src.Business.Interfaces;
using UnifiedUserSystem.src.UnifiedUserSystem.Contracts.DTOs;

namespace UnifiedUserSystem.src.Business
{
    public class UserBusiness : IUserBusiness
    {
        public void ValidateRegister(RegisterRequest req)
        {
            if(req.Password is null || req.Password.Length < 0)
            throw new ArgumentException("Password must be at least 8 characters.");
        }
    }
}
