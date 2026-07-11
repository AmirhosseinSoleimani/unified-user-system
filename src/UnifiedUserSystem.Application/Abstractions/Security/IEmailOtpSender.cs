
using UnifiedUserSystem.src.Domain.Identity.Entities;

namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public interface IEmailOtpSender
{
    Task SendAsync(User user, string otpCode, DateTimeOffset expiresAt, CancellationToken ct = default);
}
