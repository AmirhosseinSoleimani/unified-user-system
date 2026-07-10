using UnifiedUserSystem.src.Contracts.DTOs.Auth;

namespace UnifiedUserSystem.src.Application.Abstractions.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default);
    Task<LoginResponse?> LoginAsync(LoginRequest req, CancellationToken ct = default);
    Task<AuthResponse?> VerifyMfaAsync(VerifyMfaRequest req, CancellationToken ct = default);
    Task<AuthResponse?> RefreshAsync(RefreshTokenRequest req, CancellationToken ct = default);
    Task<bool> LogoutAsync(Guid currentUserId, LogoutRequest req, CancellationToken ct = default);
    Task RevokeAllSessionsAsync(Guid userId, CancellationToken ct = default);
}
