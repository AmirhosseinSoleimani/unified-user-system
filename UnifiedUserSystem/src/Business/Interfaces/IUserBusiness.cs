using UnifiedUserSystem.src.Contracts.DTOs.Auth;

namespace UnifiedUserSystem.src.Business.Interfaces
{
    public interface IUserBusiness
    {
        void ValidateRegister(RegisterRequest req);
        void ValidateLogin(LoginRequest req);
    }
}
