using UnifiedUserSystem.src.Contracts.DTOs.Auth;

namespace UnifiedUserSystem.src.Application.Validation;

public interface ILoginRequestValidator
{
    void Validate(LoginRequest request);
}