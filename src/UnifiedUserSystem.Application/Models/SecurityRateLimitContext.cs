using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedUserSystem.src.Application.Models;

public sealed record SecurityRateLimitContext(
    string PolicyName,
    string PartitionKey);
