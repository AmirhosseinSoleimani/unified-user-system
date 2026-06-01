namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public interface IRefreshTokenService
    {
        string GenerateToken();
        string HashToken(string refreshToken);
        DateTimeOffset GetExpiresAtUtc(DateTimeOffset issuedAtUtc);
    }
}
