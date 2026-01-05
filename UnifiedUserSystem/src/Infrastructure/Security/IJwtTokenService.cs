using UnifiedUserSystem.src.UnifiedUserSystem.Domain.Entities;

namespace UnifiedUserSystem.src.Infrastructure.Security
{
    public interface IJwtTokenService
    {
        string CreateAccessToken(User user);
    }
}
