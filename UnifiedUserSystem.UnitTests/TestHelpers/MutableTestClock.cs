using UnifiedUserSystem.src.Infrastructure.Time;

namespace UnifiedUserSystem.UnitTests.TestHelpers
{
    public sealed class MutableTestClock : IClock
    {
        public MutableTestClock(DateTimeOffset utcNow)
        {
            Utcnow = utcNow;
        }

        public DateTimeOffset Utcnow { get; private set; }

        public void Advance(TimeSpan value)
        {
            Utcnow = Utcnow.Add(value);
        }

        public void Set(DateTimeOffset value)
        {
            Utcnow = value;
        }
    }
}