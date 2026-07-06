namespace Academy.src.Shared.Academy.SharedKernel.Domain.Exceptions;

public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(string message) : base(message)
    {
    }

    public DomainValidationException(
        string message,
        IReadOnlyDictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; } =
        new Dictionary<string, string[]>();
}