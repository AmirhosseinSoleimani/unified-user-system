using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Contracts.DTOs.Localization;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Localization.Entities;

namespace UnifiedUserSystem.src.Application.Services.Localization;

public sealed class ErrorMessageService : IErrorMessageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizedMessageCache _localizedMessageCache;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public ErrorMessageService(
        IUnitOfWork unitOfWork,
        ILocalizedMessageCache localizedMessageCache,
        IClock clock,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _localizedMessageCache = localizedMessageCache;
        _clock = clock;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ErrorMessageResponse>> ListAsync(CancellationToken ct = default)
    {
        var messages = await _unitOfWork.ErrorMessages.ListAsync(activeOnly: false, ct);
        return messages.Select(ToResponse).ToArray();
    }

    public async Task<ErrorMessageResponse> UpsertAsync(UpsertErrorMessageRequest request, CancellationToken ct = default)
    {
        if (request is null)
            throw new DomainException("Request is null.");

        var key = ErrorMessage.NormalizeKey(request.Key);
        var now = _clock.Utcnow;
        var actorUserId = _currentUser.UserId;
        var message = await _unitOfWork.ErrorMessages.FindByKeyAsync(key, ct);

        if (message is null)
        {
            message = ErrorMessage.Create(
                key,
                request.EnglishText,
                request.PersianText,
                now,
                actorUserId);

            _unitOfWork.ErrorMessages.Add(message);
        }
        else
        {
            message.Update(
                key,
                request.EnglishText,
                request.PersianText,
                request.IsActive,
                now,
                actorUserId);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        await _localizedMessageCache.RefreshAsync(ct);

        return ToResponse(message);
    }

    public Task RefreshCacheAsync(CancellationToken ct = default)
        => _localizedMessageCache.RefreshAsync(ct);

    private static ErrorMessageResponse ToResponse(ErrorMessage message)
    {
        return new ErrorMessageResponse(
            Id: message.Id,
            Key: message.Key,
            EnglishText: message.EnglishText,
            PersianText: message.PersianText,
            IsActive: message.IsActive,
            CreatedAt: message.CreatedAt,
            UpdatedAt: message.UpdatedAt);
    }
}



