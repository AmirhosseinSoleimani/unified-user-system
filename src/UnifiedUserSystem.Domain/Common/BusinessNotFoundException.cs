namespace UnifiedUserSystem.src.Domain.Common;

public sealed class BusinessNotFoundException : KeyNotFoundException, ICodedBusinessException
{
    private BusinessNotFoundException(
        string code,
        IReadOnlyDictionary<string, object?>? parameters,
        Exception? innerException)
        : base(BusinessExceptionDiagnostics.Build(code, parameters), innerException)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("A business error code is required.", nameof(code));

        Code = code.Trim();
        Parameters = parameters ?? EmptyParameters;
    }

    public string Code { get; }

    public IReadOnlyDictionary<string, object?> Parameters { get; }

    public string? LegacyFallbackMessage => null;

    public static BusinessNotFoundException For(
        string code,
        IReadOnlyDictionary<string, object?>? parameters = null,
        Exception? innerException = null)
        => new(code, parameters, innerException);

    private static IReadOnlyDictionary<string, object?> EmptyParameters { get; }
        = new Dictionary<string, object?>();
}
