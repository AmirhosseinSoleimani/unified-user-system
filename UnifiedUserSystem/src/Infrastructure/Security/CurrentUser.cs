
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
        private ClaimsPrincipal? User => _http.HttpContext?.User;
        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public Guid? UserId
        {
            get
            {
                if (!IsAuthenticated) return null;

                var sub = User!.FindFirstValue(JwtRegisteredClaimNames.Sub);
                return (Guid.TryParse(sub, out var id))? id : null;
            }
        }
    }
}
