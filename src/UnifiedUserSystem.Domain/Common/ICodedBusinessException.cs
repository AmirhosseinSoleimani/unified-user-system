namespace UnifiedUserSystem.src.Domain.Common;

public interface ICodedBusinessException
{
    string Code { get; }

    IReadOnlyDictionary<string, object?> Parameters { get; }

    string? LegacyFallbackMessage { get; }
}

internal static class BusinessExceptionDiagnostics
{
    public static string Build(
        string code,
        IReadOnlyDictionary<string, object?>? parameters)
    {
        if (string.IsNullOrWhiteSpace(code))
            return DomainErrorCodes.DomainError;

        if (parameters is null || parameters.Count == 0)
            return code;

        var values = string.Join(
            ",",
            parameters
                .OrderBy(x => x.Key, StringComparer.Ordinal)
                .Select(x => $"{x.Key}={x.Value}"));

        return $"{code} ({values})";
    }
}
