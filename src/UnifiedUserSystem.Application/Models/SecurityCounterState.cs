namespace UnifiedUserSystem.src.Application.Models;

public sealed class SecurityCounterState
{
    public long Count { get; set; }
    public int FailedAttempts { get; set; }
    public DateTimeOffset? FirstFailedAttemptUtc { get; set; }
    public DateTimeOffset? LastFailedAttemptUtc { get; set; }
    public DateTimeOffset? LockedUntilUtc { get; set; }

    public bool IsLocked(DateTimeOffset nowUtc)
        => LockedUntilUtc.HasValue && LockedUntilUtc.Value > nowUtc;
}