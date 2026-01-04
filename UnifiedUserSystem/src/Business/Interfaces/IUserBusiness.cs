using UnifiedUserSystem.src.UnifiedUserSystem.Contracts.DTOs;

namespace UnifiedUserSystem.src.Business.Interfaces
{
    public interface IUserBusiness
    {
        void ValidateRegister(RegisterRequest req);
    }
}
