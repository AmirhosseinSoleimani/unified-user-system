namespace Academy.src.Shared.Academy.SharedKernel.Infrastructure.Time;

public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }
}

