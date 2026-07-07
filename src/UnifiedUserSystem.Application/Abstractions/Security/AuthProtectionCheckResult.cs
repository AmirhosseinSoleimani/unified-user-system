namespace UnifiedUserSystem.src.Application.Abstractions.Security;

public sealed record AuthProtectionCheckResult(bool IsAllowed)
{
    public static AuthProtectionCheckResult Allow() => new(true);
    public static AuthProtectionCheckResult Block() => new(false);
}