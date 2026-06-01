namespace UnifiedUserSystem.src.Application.Interfaces.Security
{
    public interface IRefreshTokenService
    {
        string GenerateToken();
        string HashToken(string refreshToken);
        DateTimeOffset GetExpiresAtUtc(DateTimeOffset issuedAtUtc);
    }
}
