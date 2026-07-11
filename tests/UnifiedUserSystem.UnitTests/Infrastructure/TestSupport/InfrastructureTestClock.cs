using UnifiedUserSystem.src.Application.Abstractions.Time;

namespace UnifiedUserSystem.UnitTests.Infrastructure.TestSupport;

public sealed class InfrastructureTestClock : IClock
{
    public DateTimeOffset Utcnow { get; set; } =
        new(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
}