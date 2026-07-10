namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public sealed record AuthProtectionCheckResult(
    bool IsAllowed,
    TimeSpan? RetryAfter,
    string Reason)
{
    public static AuthProtectionCheckResult Allow()
        => new(true, null, "Allowed");

    public static AuthProtectionCheckResult Block(TimeSpan? retryAfter = null, string reason = "Authentication is temporarily blocked.")
        => new(false, retryAfter, reason);
}