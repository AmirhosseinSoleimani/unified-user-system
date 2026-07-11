
using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Domain.Security.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Persistence
{
    public interface IIpSecurityEventRepository
    {
        void Add(IpSecurityEvent securityEvent);

        Task<IReadOnlyList<IpSecurityEvent>> SearchAsync(
            IpSecurityEventSearchCriteria criteria,
            CancellationToken ct = default);
    }
}
