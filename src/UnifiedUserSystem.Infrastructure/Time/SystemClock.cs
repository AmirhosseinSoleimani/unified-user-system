namespace UnifiedUserSystem.src.Infrastructure.Time
{
    public class SystemClock : IClock
    {
        public DateTimeOffset Utcnow => DateTimeOffset.UtcNow;
    }
}
