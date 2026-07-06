namespace Academy.src.Shared.Academy.SharedKernel.Domain.Exceptions;

public sealed class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }
}
