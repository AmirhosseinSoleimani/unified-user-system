using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _otp;
        private readonly IClock _clock;
        public JwtTokenService(IOptions<JwtOptions> otp, IClock clock)
        {
            _otp = otp.Value;
            _clock = clock;
        }
        public string CreateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new (JwtRegisteredClaimNames.Email, user.Email),
                new ("username", user.Username),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var userRole in user.UserRoles)
            {
                var roleName = userRole.Role?.Name;
                if(!string.IsNullOrWhiteSpace(roleName))
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var keyBytes = Encoding.UTF8.GetBytes(_otp.Key);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _otp.Issuer,
                audience: _otp.Audience,
                claims: claims,
                expires: _clock.Utcnow.AddMinutes(_otp.ExpiresMinutes).UtcDateTime,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
