using System;
namespace UnifiedUserSystem.src.Application.Validation;

public interface IPasswordPolicy
{
    void Validate(string password);
}
