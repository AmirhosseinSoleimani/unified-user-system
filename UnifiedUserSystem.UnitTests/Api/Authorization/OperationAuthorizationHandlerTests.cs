using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UnifiedUserSystem.src.Api.Authorization;
using UnifiedUserSystem.src.Application.Interfaces;
using UnifiedUserSystem.src.Domain.Authorization.Entities;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using UnifiedUserSystem.src.Infrastructure.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Api.Authorization
{
    public class OperationAuthorizationHandlerTests
    {
        private static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var currentUser = new Mock<ICurrentUser>();
            var clock = new Mock<IClock>();

            clock.Setup(x => x.Utcnow).Returns(DateTimeOffset.UtcNow);

            return new AppDbContext(options, currentUser.Object, clock.Object);
        }

        private static AuthorizationHandlerContext CreateContext(string? subClaim)
        {
            var claims = new List<Claim>();

            if (subClaim != null)
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, subClaim));

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            return new AuthorizationHandlerContext(
                new[] { new OperationRequirement("operation.test") },
                user,
                null);
        }

        [Fact]
        public async Task Should_Fail_When_User_Is_Anonymous()
        {
            var db = CreateDbContext();
            var handler = new OperationAuthorizationHandler(db);

            var context = new AuthorizationHandlerContext(
                new[] { new OperationRequirement("operation.test") },
                new ClaimsPrincipal(new ClaimsIdentity()), // anonymous
                null);

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public async Task Should_Fail_When_Sub_Claim_Is_Invalid()
        {
            var db = CreateDbContext();
            var handler = new OperationAuthorizationHandler(db);

            var context = CreateContext("invalid-guid");

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public async Task Should_Fail_When_User_Does_Not_Have_Operation()
        {
            var db = CreateDbContext();

            var user = User.CreateNew("a@a.com", "user1", "User One", "hash", DateTimeOffset.UtcNow, null);

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var handler = new OperationAuthorizationHandler(db);
            var context = CreateContext(user.Id.ToString());

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public async Task Should_Succeed_When_User_Has_Operation_Through_Role()
        {
            var db = CreateDbContext();
            var now = DateTimeOffset.UtcNow;

            var user = User.CreateNew("a@a.com", "user1", "User One", "hash", now, null);

            var role = Role.Create("admin", "Admin", now, null);

            // manually set role Id (needed for FK)
            typeof(Role).GetProperty("Id")!.SetValue(role, 1);

            var operation = Operation.Create("operation.test", "Test", now, null);

            var roleOperation = RoleOperation.Create(role.Id, operation.Id, now, null);
            var userRole = UserRole.Create(user.Id, role.Id, now, null);

            db.Users.Add(user);
            db.Roles.Add(role);
            db.Operation.Add(operation);
            db.RoleOperations.Add(roleOperation);
            db.UserRoles.Add(userRole);

            await db.SaveChangesAsync();

            var handler = new OperationAuthorizationHandler(db);
            var context = CreateContext(user.Id.ToString());

            await handler.HandleAsync(context);

            context.HasSucceeded.Should().BeTrue();
        }
    }
}
