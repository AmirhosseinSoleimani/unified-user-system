using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public sealed record DistributedRateLimitLeaseResult(
    bool IsAcquired,
    bool IsUnavailable,
    long Count,
    TimeSpan? RetryAfter,
    string? Reason)
{
    public static DistributedRateLimitLeaseResult Acquired(long count, TimeSpan? retryAfter = null)
        => new(true, false, count, retryAfter, null);

    public static DistributedRateLimitLeaseResult Rejected(long count, TimeSpan? retryAfter, string reason)
        => new(false, false, count, retryAfter, reason);

    public static DistributedRateLimitLeaseResult Unavailable(string reason)
        => new(false, true, 0, null, reason);
}