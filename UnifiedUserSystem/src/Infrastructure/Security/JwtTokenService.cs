using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _otp;
        public JwtTokenService(IOptions<JwtOptions> otp)
        {
            _otp = otp.Value;
        }
        public string CreateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("username", user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var keyBytes = Encoding.UTF8.GetBytes(_otp.Key);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_otp.ExpiresMinutes);
            var token = new JwtSecurityToken(
                issuer: _otp.Issuer,
                audience: _otp.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
