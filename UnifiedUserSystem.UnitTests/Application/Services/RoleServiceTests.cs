//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Moq;
//using UnifiedUserSystem.src.Application.Interfaces;
//using UnifiedUserSystem.src.Application.Interfaces.Identity;
//using UnifiedUserSystem.src.Application.Services;
//using UnifiedUserSystem.src.Domain.Identity.Entities;
//using UnifiedUserSystem.src.Infrastructure.Time;

//namespace UnifiedUserSystem.UnitTests.Application.Services
//{
//    public class RoleServiceTests
//    {
//        private static readonly DateTimeOffset Now = new DateTimeOffset(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
//        private static readonly Guid Actor = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

//        private readonly Mock<IUnitOfWork> _uow = new();
//        private readonly Mock<IRoleRepository> _roles = new();
//        private readonly Mock<IUserRepository> _users = new();
//        private readonly Mock<IClock> _clock = new();
//        private readonly Mock<ICurrentUser> _currentUser = new();

//        public RoleServiceTests CreateSut()
//        {
//            _uow.SetupGet(x => x.Roles).Returns(_roles.Object);
//            _uow.SetupGet(x => x.Users).Returns(_users.Object);

//            _clock.SetupGet(x => x.Utcnow).Returns(Now);
//            _currentUser.SetupGet(x => x.UserId).Returns(Actor);

//            return new RoleService(_uow.Object, _clock.Object, _currentUser.Object);
//        }

//        #region

//        [Fact]
//        public async Task CreateRoleAsync_WhenNameAlreadyExists_ShouldThrow_AndNotSave()
//        {
//            var ct = CancellationToken.None;
//            var sut = CreateSut();

//            _roles.Setup(r => r.FindByNameAsync("Admin", ct))
//                .ReturnsAsync(Role.Create("admin", "Admin", Now, Actor));

//            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateRoleAsync(" Admin ", ct));
//            Assert.Contains("already exists", ex.Message, StringComparison.OrdinalIgnoreCase);

//            _roles.Verify(r => r.Add(It.IsAny<Role>()), Times.Never);
//            _uow.Verify(u => u.SaveChangesAsync(ct), Times.Never);
//        }
//        #endregion
//    }
//}