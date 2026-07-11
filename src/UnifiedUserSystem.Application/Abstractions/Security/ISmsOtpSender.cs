
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public interface ISmsOtpSender
{
    Task SendAsync(User user, string otpCode, DateTimeOffset expiresAt, CancellationToken ct = default);
}
