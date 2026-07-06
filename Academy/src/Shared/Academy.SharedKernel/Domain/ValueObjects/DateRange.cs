using Academy.src.Shared.Academy.SharedKernel.Domain.Exceptions;
using Academy.src.Shared.Academy.SharedKernel.Domain.Primitives;

namespace Academy.src.Shared.Academy.SharedKernel.Domain.ValueObjects;

public sealed class DateRange : ValueObject
{
    private DateRange(DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc)
    {
        StartsAtUtc = startsAtUtc;
        EndsAtUtc = endsAtUtc;
    }

    public DateTimeOffset StartsAtUtc { get; }
    public DateTimeOffset EndsAtUtc { get; }

    public static DateRange Create(DateTimeOffset startsAtUtc, DateTimeOffset endsAtUtc)
    {
        if (endsAtUtc < startsAtUtc)
            throw new DomainValidationException("End date must be greater than or equal to start date.");

        return new DateRange(startsAtUtc, endsAtUtc);
    }

    public bool Contains(DateTimeOffset dateTimeUtc)
    {
        return dateTimeUtc >= StartsAtUtc && dateTimeUtc <= EndsAtUtc;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartsAtUtc;
        yield return EndsAtUtc;
    }
}
