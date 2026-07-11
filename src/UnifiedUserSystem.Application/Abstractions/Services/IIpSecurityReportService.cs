using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Contracts.DTOs.Security;

namespace UnifiedUserSystem.src.Application.Abstractions.Services;

public interface IIpSecurityReportService
{
    Task<IReadOnlyList<IpSecurityEventResponse>> SearchAsync(
        IpSecurityReportQuery query,
        CancellationToken ct = default);
}
