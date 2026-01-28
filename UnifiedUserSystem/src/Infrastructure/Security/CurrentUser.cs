
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UnifiedUserSystem.src.Application.Interfaces;

namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _http;
        public CurrentUser(IHttpContextAccessor http) 
        {
            _http = http;
        }
        public Guid? UserId 
        { 
            get 
            {
                var user = _http.HttpContext?.User;

                if (user?.Identity?.IsAuthenticated != true) return null;

                var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
                if (Guid.TryParse(sub, out var id)) return id;
                return null;
            }
        }
    }
}
