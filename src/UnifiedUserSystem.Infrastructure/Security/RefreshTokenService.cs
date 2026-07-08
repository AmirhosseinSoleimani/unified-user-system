using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly RefreshTokenOptions _options;

        public RefreshTokenService(IOptions<RefreshTokenOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateToken()
        {
            if (_options.ExpiresDays <= 0)
                throw new InvalidOperationException("Refresh token expiration must be greater than zero.");

            if (_options.TokenSizeBytes <= 0)
                throw new InvalidOperationException("Refresh token size must be greater than zero.");

            var bytes = RandomNumberGenerator.GetBytes(_options.TokenSizeBytes);
            return Base64UrlEncoder.Encode(bytes);
        }

        public string HashToken(string refreshToken)
        {
            var normalized = Guard.NotEmpty(refreshToken, nameof(refreshToken));
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));

            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        public DateTimeOffset GetExpiresAtUtc(DateTimeOffset issuedAtUtc)
        {
            if (_options.ExpiresDays <= 0)
                throw new InvalidOperationException("Refresh token expiry must be greater than zero.");

            return issuedAtUtc.AddDays(_options.ExpiresDays);
        }
    }
}
