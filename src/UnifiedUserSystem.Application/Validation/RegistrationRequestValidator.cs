using UnifiedUserSystem.src.Application.Validation;
using UnifiedUserSystem.src.Contracts.DTOs.Auth;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;

public sealed class RegistrationRequestValidator : IRegistrationRequestValidator
{
    private readonly IPasswordPolicy _passwordPolicy;

    public RegistrationRequestValidator(IPasswordPolicy passwordPolicy)
    {
        _passwordPolicy = passwordPolicy;
    }

    public void Validate(RegisterRequest request)
    {
        if (request is null)
            throw new DomainException("Request is null.");

        var email = Guard.NotEmpty(request.Email, nameof(request.Email));
        var username = Guard.NotEmpty(request.Username, nameof(request.Username));
        var firstName = request.FirstName;
        var lastName = request.LastName;
        var phoneNumber = Guard.NotEmpty(request.PhoneNumber, nameof(request.PhoneNumber));
        var password = Guard.NotEmpty(request.Password, nameof(request.Password));

        User.CreateNew(
            email,
            username,
            firstName,
            lastName,
            phoneNumber,
            passwordHash: "application-validation-placeholder",
            DateTimeOffset.UnixEpoch,
            actorUserId: null);

        _passwordPolicy.Validate(password);
    }
}
