namespace UnifiedUserSystem.src.Infrastructure.Time
{
    public interface IClock
    {
        DateTimeOffset Utcnow { get; }
    }
}
