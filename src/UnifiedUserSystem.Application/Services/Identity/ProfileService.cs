using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Options;
using UnifiedUserSystem.src.Contracts.DTOs.Profile;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace UnifiedUserSystem.src.Application.Services.Identity;

public class ProfileService : IProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly LocalizationOptions _localizationOptions;

    public ProfileService(IUnitOfWork unitOfWork, ICurrentUser currentUser)
        : this(
              unitOfWork,
              currentUser,
              new SystemClock(),
              OptionsFactory.Create(new LocalizationOptions()))
    {
    }

    public ProfileService(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        IClock clock,
        IOptions<LocalizationOptions> localizationOptions)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _clock = clock;
        _localizationOptions = localizationOptions.Value;
    }

    public async Task<ProfileResponse> GetMyProfileAsync(CancellationToken ct = default)
    {
        var user = await GetCurrentUserAsync(ct);
        return Map(user);
    }

    public async Task<ProfileResponse> UpdatePreferredLocaleAsync(
    UpdatePreferredLocaleRequest request,
    CancellationToken ct = default)
    {
        if (request is null)
        {
            throw DomainException.For(DomainErrorCodes.RequestRequired);
        }

        var preferredLocale = _localizationOptions.NormalizeSupportedLocale(request.PreferredLocale);


        if (preferredLocale is null)
        {
            throw DomainException.For(
                DomainErrorCodes.UnsupportedLocale,
                new Dictionary<string, object?>
                {
                    ["locale"] = request.PreferredLocale,
                    ["supportedLocales"] = string.Join(", ", _localizationOptions.SupportedLocales)
                });
        }

        var user = await GetCurrentUserAsync(ct);
        user.ChangePreferredLocale(preferredLocale, _clock.Utcnow, user.Id);
        await _unitOfWork.SaveChangesAsync(ct);

        return Map(user);
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated ||
            _currentUser.UserId is not Guid userId ||
            userId == Guid.Empty)
        {
            throw new UnauthorizedAccessException();
        }

        return await _unitOfWork.Users.FindByIdWithRolesAsync(userId, ct)
               ?? throw BusinessNotFoundException.For(DomainErrorCodes.UserNotFound);
    }

    private static ProfileResponse Map(User user)
    {
        var roles = user.UserRoles
            .Where(x => x.Role is not null)
            .Select(x => x.Role.Name)
            .Distinct()
            .ToArray();

        return new ProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Fullname = user.Fullname,
            IsActive = user.IsActive,
            Roles = roles,
            PreferredLocale = user.PreferredLocale
        };
    }

    private sealed class SystemClock : IClock
    {
        public DateTimeOffset Utcnow => DateTimeOffset.UtcNow;
    }
}
