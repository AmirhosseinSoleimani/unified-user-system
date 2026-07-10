using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public interface IDistributedRateLimitStore
{
    Task<DistributedRateLimitLeaseResult> TryAcquireAsync(
        string key,
        int permitLimit,
        TimeSpan window,
        TimeSpan? cooldown,
        CancellationToken cancellationToken = default);

    Task ResetAsync(string key, CancellationToken cancellationToken = default);
}
