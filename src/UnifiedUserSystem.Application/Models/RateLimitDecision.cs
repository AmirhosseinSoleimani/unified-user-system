using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedUserSystem.src.Application.Models;

public sealed record RateLimitDecision(
    bool IsAllowed,
    TimeSpan? RetryAfter,
    string Reason)
{
    public static RateLimitDecision Allowed()
        => new(true, null, "Allowed");

    public static RateLimitDecision Rejected(TimeSpan? retryAfter, string reason)
        => new(false, retryAfter, reason);
}
