using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using Xunit;

namespace UnifiedUserSystem.tests.UnifiedUserSystem.UnitTests.Domain.Register
{
    public class UserTests
    {

        private static DateTimeOffset Now() => DateTimeOffset.Now;

        private static string LongString(int len) => new string('a', len);

        [Fact]
        public void NormalizeUsername_ShouldTrim_AndHandleNull()
        {
            Assert.Equal("", User.NormalizeUsername(null!));
            Assert.Equal("ali", User.NormalizeUsername(" ali "));
        }

        [Fact]
        public void NormalizeEmail_ShouldTrim_Lower_AndHandleNull()
        {
            Assert.Equal("", User.NormalizeEmail(null!));
            Assert.Equal("test@example.com", User.NormalizeEmail(" TEST@example.com "));
        }

        [Fact]
        public void NormalizeFullname_ShouldTrim_AndHandleNull()
        {
            Assert.Equal("", User.NormalizeFullname(null!));
            Assert.Equal("Ali Reza", User.NormalizeUsername(" Ali Reza "));
        }

        [Fact]
        public void CreateNew_WhenEmailEmpty_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() => 
                User.CreateNew("  ", "ali", "Ali", "HASH", Now(), null));

            Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("a@")]
        [InlineData("@b.com")]
        [InlineData("a@b")]
        [InlineData("a b@c.com")]
        public void CreateNew_WhenEmailInvalidFormat_ShouldThrow(string email)
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew(email, "ali", "Ali", "HASH", Now(), null));

            Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("format", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenEmailTooLong_ShouldThrow()
        {
            var email = $"{LongString(User.EmailMaxLength)}@a.com";
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew(email, "ali", "Ali", "HASH", Now(), null));

            Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("abcdefghijklmnopqrstuvwxyz")]
        public void CreateNew_WhenUsernameLengthInvalid_ShouldThrow(string username)
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", username, "Ali", "HASH", Now(), null));

            Assert.Contains("username", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("between", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("admin")]
        [InlineData("ROOT")]
        [InlineData("Api")]
        public void CreateNew_WhenUsernameReserved_ShouldThrow(string username)
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", username, "Ali", "HASH", Now(), null));

            Assert.Contains("reserved", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("1ali")]
        [InlineData("_ali")]
        [InlineData(".ali")]
        [InlineData(("ali-xx"))]
        [InlineData("ali..xx")]
        [InlineData("ali.")]
        [InlineData("al i")]
        public void CreateNew_WhenUsernameFormatInvalid_ShouldThrow(string username)
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", username, "Ali", "HASH", Now(), null));

            Assert.Contains("username", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("invalid", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenFullnameEmpty_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", "ali", " ", "HASH", Now(), null));

            Assert.Contains("fullname", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenFullnameTooLong_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", "ali", LongString(User.FullnameMaxLength + 1), "HASH", Now(), null));

            Assert.Contains("fullname", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenPasswordHashEmpty_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", "ali", "Ali", " ", Now(), null));

            Assert.Contains("passwordHash", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenPasswordHashTooLong_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", "ali", "Ali", LongString(User.PasswordHashMaxLength + 1), Now(), null));

            Assert.Contains("passwordHash", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_ShouldNormalizeEmail_And_TrimFields_AndInitCollections()
        {
            var now = Now();

            var user = User.CreateNew(
                email: " TEST@Example.com ",
                username: " ali ",
                fullname: " Ali Reza ",
                passwordHash: "HASH",
                nowUtc: now,
                actorUserId: null
                );

            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("ali", user.Username);
            Assert.Equal("Ali Reza", user.Fullname);
            Assert.Equal("HASH", user.PasswordHash);

            Assert.True(user.IsActive);
            Assert.NotEqual(Guid.Empty, user.Id);

            Assert.NotNull(user.UserRoles);
            Assert.Empty(user.UserRoles);

            Assert.Equal(now, user.CreatedAt);
            Assert.Equal(now, user.UpdatedAt);

            Assert.Equal(user.Id, user.CreatedByUserId);
            Assert.Equal(user.Id, user.UpdatedByUserId);
        }

        [Fact]
        public void ChangeFullName_ShouldTrim_AndUpdateAudit()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = User.CreateNew("a@b.com", "ali", "Old", "HASH", t1, null);

            user.ChangeFullName(" New Name ", t2, user.Id);

            Assert.Equal("New Name", user.Fullname);
            Assert.Equal(t2, user.UpdatedAt);
            Assert.Equal(user.Id, user.UpdatedByUserId);
        }

        [Fact]
        public void ChangeFullName_WhenEmpty_ShouldThrow()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);

            var ex = Assert.Throws<DomainException>(() =>
                user.ChangeFullName("  ", t2, user.Id));

            Assert.Contains("fullname", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeFullName_WhenTooLong_ShouldThrow()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);

            var ex = Assert.Throws<DomainException>(() => 
                user.ChangeFullName(LongString(User.FullnameMaxLength + 1), t2, user.Id));

            Assert.Contains("fullname", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeFullName_WhenSameValue_ShouldNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = User.CreateNew("a@b.com", "ali", "Same", "HASH", t1, null);
            var before = user.UpdatedAt;

            user.ChangeFullName(" Same ", t2, user.Id);

            Assert.Equal("Same", user.Fullname);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Fact]
        public void ChangePasswordHash_WhenEmpty_ShouldThrow()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);
            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);

            var ex = Assert.Throws<DomainException>(() =>
                user.ChangePasswordHash("  ", t2, user.Id));

            Assert.Contains("passwordHash", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangePasswordHash_WhenTooLong_ShouldThrow()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);

            var ex = Assert.Throws<DomainException>(() =>
                user.ChangePasswordHash(LongString(User.PasswordHashMaxLength + 1), t2, user.Id));

            Assert.Contains("passwordHash", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangePsswordHash_ShouldUpdateHash_AndAudit()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH1", t1, null);

            user.ChangePasswordHash("HASH2", t2, user.Id);

            Assert.Equal("HASH2", user.PasswordHash);
            Assert.Equal(t2, user.UpdatedAt);
            Assert.Equal(user.Id, user.UpdatedByUserId);
        }

        [Fact]
        public void ChangePasswordHash_WhenSameValue_ShouldNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);
            var before = user.UpdatedAt;

            user.ChangePasswordHash("HASH", t2, user.Id);

            Assert.Equal("HASH", user.PasswordHash);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Fact]
        public void Deactive_ShouldSetIsActiveFalse_AndTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);
            Assert.True(user.IsActive);

            user.Deactive(t2, user.Id);

            Assert.False(user.IsActive);
            Assert.Equal(t2, user.UpdatedAt);
        }

        [Fact]
        public void Deactive_WhenAlreadyInactive_ShouldNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);
            var t3 = t2.AddMinutes(1);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);

            user.Deactive(t2, user.Id);
            var before = user.UpdatedAt;

            user.Deactive(t3, user.Id);

            Assert.False(user.IsActive);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Fact]
        public void Active_ShouldSetIsActiveTrue_AndTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);
            var t3 = t2.AddMinutes(1);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);

            user.Deactive(t2, user.Id);
            Assert.False(user.IsActive);

            user.Active(t3, user.Id);
            Assert.True(user.IsActive);
            Assert.Equal(t3, user.UpdatedAt);
        }

        [Fact]
        public void Active_WhenAlreadyActive_ShouldNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);
            var before = user.UpdatedAt;

            user.Active(t2, user.Id);

            Assert.True(user.IsActive);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Fact]
        public void AssignRole_WhenRoleIdInvalid_ShouldThrow()
        {
            var now = Now();
            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", now, null);

            var ex = Assert.Throws<DomainException>(() =>
                user.AssignRole(0, now, user.Id));

            Assert.Contains("roleId", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AssignRole_ShouldAddRole_AndTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);

            user.AssignRole(1, t2, user.Id);

            Assert.Single(user.UserRoles);
            Assert.Equal(1, user.UserRoles.First().RoleId);
            Assert.Equal(t2, user.UpdatedAt);
        }

        [Fact]
        public void AssignRole_WhenDuplicate_ShouldNotAdd_AndNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);
            var t3 = t2.AddMinutes(1);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);

            user.AssignRole(1, t2, user.Id);
            var before = user.UpdatedAt;

            user.AssignRole(1, t3, user.Id);

            Assert.Single(user.UserRoles);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Fact]
        public void RemoveRole_WhenRoleIdInValid_ShouldThrowArgumentException()
        {
            var now = Now();
            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", now, null);

            var ex = Assert.Throws<DomainException>(() =>
                user.RemoveRole(0, now, user.Id));

            Assert.Equal("RoleId is invalid.", ex.Message, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void RemoveRole_WhenRoleNotFound_ShouldNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);
            var before = user.UpdatedAt;

            user.RemoveRole(99, t2, user.Id);

            Assert.Empty(user.UserRoles);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Fact]
        public void RemoveRole_ShouldRemove_AndTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);
            var t3 = t2.AddMinutes(1);

            var user = User.CreateNew("a@b.com", "ali", "Name", "HASH", t1, null);
            user.AssignRole(1, t2, user.Id);

            user.RemoveRole(1, t3, user.Id);

            Assert.Empty(user.UserRoles);
            Assert.Equal(t3, user.UpdatedAt);
        }


    }
}
