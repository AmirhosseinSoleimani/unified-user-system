using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public interface IJwtTokenService
    {
        string CreateAccessToken(User user);
    }
}
