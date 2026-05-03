//using Moq;
//using FluentAssertions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnifiedUserSystem.src.Application.Interfaces;
//using UnifiedUserSystem.src.Application.Interfaces.Identity;
//using UnifiedUserSystem.src.Application.Services.Identity;

//namespace UnifiedUserSystem.UnitTests.Application.Services
//{
//    public class ProfileServiceTests
//    {
//        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
//        private readonly Mock<IUserRepository> _userRepositoryMock = new();
//        private readonly Mock<ICurrentUser> _currentUserMock = new();

//        private readonly ProfileService _profileService;

//        public ProfileServiceTests()
//        {
//            _unitOfWorkMock.SetupGet(x => x.Users).Returns(_userRepositoryMock.Object);

//            _profileService = new ProfileService(
//                _unitOfWorkMock.Object,
//                _currentUserMock.Object);
//        }

//        [Fact]
//        public async Task GetMyProfileAsync_WhenUserIdIsNull_ShouldThrowUnauthorizedAccessException()
//        {
//            _currentUserMock.SetupGet(x => x.IsAuthenticated).Returns(true);
//            _currentUserMock.SetupGet(x => x.UserId).Returns((Guid?)null);

//            Func<Task> act = async () => await _profileService.GetMyProfileAsync();

//            await act.Should().ThrowAsync<UnauthorizedAccessException>();
//        }
//    }
//}
