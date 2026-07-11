using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.src.Application.Validation;

public sealed class LoginRequestValidator : ILoginRequestValidator
{
    public void Validate(LoginRequest request)
    {
        if (request is null)
            throw new DomainException("Request is null.");

        Guard.NotEmpty(request.EmailOrUsername, nameof(request.EmailOrUsername));
        Guard.NotEmpty(request.Password, nameof(request.Password));
    }
}
