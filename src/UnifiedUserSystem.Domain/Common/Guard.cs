using System.Reflection.Metadata;

namespace UnifiedUserSystem.src.Domain.Common;

public class Guard
{
    public static void NotNull(object? value, string name)
    {
        if (value is not null) return;
        
        throw DomainException.For(
            DomainErrorCodes.ValidationNull,
            Parameters(("field", name)));
    }

    public static string NotEmpty(string? value, string name)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(normalized))
            return normalized;

        throw DomainException.For(
            DomainErrorCodes.ValidationRequired,
            Parameters(("field", name)));
    }

    public static void MaxLen(string value, int max, string name)
    {
        if (value.Length <= max)
            return;

        throw DomainException.For(
            DomainErrorCodes.ValidationMaxLength,
            Parameters(("field", name), ("max", max)));
    }

    public static void MinLen(string value, int min, string name)
    {
        if (value.Length >= min)
            return;

        throw DomainException.For(
            DomainErrorCodes.ValidationMinLength,
            Parameters(("field", name), ("min", min)));
    }

    public static void AllowedLen(string value, int max, int min, string name)
    {
        if (value.Length >= min && value.Length <= max)
            return;

        throw DomainException.For(
            DomainErrorCodes.ValidationLengthRange,
            Parameters(("field", name), ("min", min), ("max", max)));
    }

    public static void True(bool condition, string message)
    {
        if (condition)
            return;
        throw new DomainException(message);
    }

    public static void True(
        bool condition,
        string code,
        IReadOnlyDictionary<string, object?>? parameters = null)
    {
        if (condition)
            return;

        throw DomainException.For(code, parameters);
    }

    private static IReadOnlyDictionary<string, object?> Parameters(
        params (string Key, object? Value)[] values)
        => values.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
}
