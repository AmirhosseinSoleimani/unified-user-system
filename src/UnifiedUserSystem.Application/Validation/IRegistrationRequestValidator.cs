using UnifiedUserSystem.src.Contracts.DTOs.Auth;

namespace UnifiedUserSystem.src.Application.Validation;

public interface IRegistrationRequestValidator
{
    void Validate(RegisterRequest request);
}