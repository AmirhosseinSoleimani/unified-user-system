using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Contracts.DTOs.Profile;

namespace UnifiedUserSystem.src.Application.Services.Identity
{
    public class ProfileService : IProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUser _currentUser;

        public ProfileService(IUnitOfWork unitOfWork, ICurrentUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }
        public async Task<ProfileResponse> GetMyProfileAsync(CancellationToken ct = default)
        {
            if (!_currentUser.IsAuthenticated || _currentUser.UserId.HasValue)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }
            var user = await _unitOfWork.Users.FindByIdWithRolesAsync(_currentUser.UserId.Value, ct);

            if (user is null) 
            {
                throw new KeyNotFoundException("User not found.");
            }

            var roles = user.UserRoles
                .Where(x => x.Role != null)
                .Select(x => x.Role.Name)
                .Distinct()
                .ToArray();

            return new ProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                Fullname = user.Fullname,
                IsActive = user.IsActive,
                Roles = roles
            };
        }
    }
}
