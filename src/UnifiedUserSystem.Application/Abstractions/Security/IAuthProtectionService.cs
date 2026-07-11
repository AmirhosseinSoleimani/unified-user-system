using UnifiedUserSystem.src.Application.Abstractions.Web;

namespace UnifiedUserSystem.src.Application.Abstractions.Security
{
    public interface IAuthProtectionService
    {
        Task<AuthProtectionCheckResult> CheckAsync(
            string normalizedLoginIdentifier,
            IClientContext clientContext,
            CancellationToken ct = default);

        Task RecordFailureAsync(
            string normalizedLoginIdentifier,
            IClientContext clientContext,
            CancellationToken ct = default);

        Task ResetAsync(
            string normalizedLoginIdentifier,
            IClientContext clientContext,
            CancellationToken ct = default);
    }
}