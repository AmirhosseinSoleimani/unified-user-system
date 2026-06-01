
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UnifiedUserSystem.src.Application.Interfaces.Security;
using UnifiedUserSystem.src.Domain.Identity.Entities;

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

                var userId =
                     User!.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                     User.FindFirstValue("sub") ??
                     User.FindFirstValue(ClaimTypes.NameIdentifier);

                return Guid.TryParse(userId, out var id) ? id : null;
            }
        }
    }
}
