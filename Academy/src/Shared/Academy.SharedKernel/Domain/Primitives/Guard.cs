using Academy.src.Shared.Academy.SharedKernel.Domain.Exceptions;

namespace Academy.src.Shared.Academy.SharedKernel.Domain.Primitives;

public static class Guard
{
    public static string AgainstNullOrWhiteSpace(
        string? value,
        string parameterName,
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException(message ?? $"{parameterName} is required.");

        return value;
    }

    public static Guid AgainstEmptyGuid(
        Guid value,
        string parameterName,
        string? message = null)
    {
        if (value == Guid.Empty)
            throw new DomainValidationException(message ?? $"{parameterName} is required.");

        return value;
    }

    public static long AgainstZeroOrNegative(
        long value,
        string parameterName,
        string? message = null)
    {
        if (value <= 0)
            throw new DomainValidationException(message ?? $"{parameterName} must be greater than zero.");

        return value;
    }

    public static int AgainstZeroOrNegative(
        int value,
        string parameterName,
        string? message = null)
    {
        if (value <= 0)
            throw new DomainValidationException(message ?? $"{parameterName} must be greater than zero.");

        return value;
    }

    public static T AgainstNull<T>(
        T? value,
        string parameterName,
        string? message = null)
        where T : class
    {
        if (value is null)
            throw new DomainValidationException(message ?? $"{parameterName} is required.");

        return value;
    }
}
