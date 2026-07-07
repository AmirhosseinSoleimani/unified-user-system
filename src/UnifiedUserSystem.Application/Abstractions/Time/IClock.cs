namespace UnifiedUserSystem.src.Application.Abstractions.Time;

public interface IClock
{
    DateTimeOffset Utcnow { get; }
}
