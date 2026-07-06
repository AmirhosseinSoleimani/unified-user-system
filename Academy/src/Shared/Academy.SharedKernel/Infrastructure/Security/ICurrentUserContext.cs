namespace Academy.src.Shared.Academy.SharedKernel.Infrastructure.Security;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
}

