using UnifiedUserSystem.src.Application.Interfaces;

namespace UnifiedUserSystem.src.Application.Interfaces.Security
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