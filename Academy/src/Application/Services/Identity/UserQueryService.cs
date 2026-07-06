using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Application.Interfaces.Identity;
using UnifiedUserSystem.src.Contracts.DTOs.Profile;
using UnifiedUserSystem.src.Contracts.DTOs.Users;

namespace UnifiedUserSystem.src.Application.Services.Identity
{
    public class UserQueryService : IUserQueryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserQueryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProfileResponse> GetUserByIdAsync(Guid id, CancellationToken ct = default)
        {
            var user = await _unitOfWork.Users.FindByIdWithRolesAsync(id, ct);

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

        public async Task<IReadOnlyList<ActiveUserListItemResponse>> ListActiveUsersAsync(CancellationToken ct = default)
        {
            var users = await _unitOfWork.Users.ListActiveAsync(ct);
            return users
                .Select(user => new ActiveUserListItemResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    Fullname = user.Fullname,
                    IsActive = user.IsActive,
            }).ToList();
        }
    }
}
