using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedUserSystem.src.Application.Options;

public sealed class SecurityRuntimeOptions
{
    public string RedisUnavailableMode { get; set; } = "FailClosed";
    public string RateLimitKeyPrefix { get; set; } = "UnifiedUserSystem";
}
