using UnifiedUserSystem.src.Contracts.DTOs;

namespace UnifiedUserSystem.src.UnifiedUserSystem.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest req);
        Task<AuthResponse?> LoginAsync(LoginRequest req);
    }
}
