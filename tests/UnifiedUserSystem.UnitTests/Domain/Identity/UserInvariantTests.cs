using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Identity.Entities;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Domain.Identity.Entities
{
    public class UserTests
    {
        private static DateTimeOffset Now() => DateTimeOffset.Now;

        private static string LongString(int len) => new string('a', len);

        private static User CreateValidUser(DateTimeOffset? now = null)
        {
            return User.CreateNew(
                email: "a@b.com",
                username: "ali",
                firstName: "Ali",
                lastName: "Rezaei",
                phoneNumber: "+989123456789",
                passwordHash: "HASH",
                nowUtc: now ?? Now(),
                actorUserId: null);
        }

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
        public void NormalizeFirstName_ShouldTrim_AndHandleNull()
        {
            Assert.Equal("", User.NormalizeFirstName(null!));
            Assert.Equal("Ali", User.NormalizeFirstName(" Ali "));
        }

        [Fact]
        public void NormalizeLastName_ShouldTrim_AndHandleNull()
        {
            Assert.Equal("", User.NormalizeLastName(null!));
            Assert.Equal("Rezaei", User.NormalizeLastName(" Rezaei "));
        }

        [Fact]
        public void NormalizePhoneNumber_ShouldTrim_AndHandleNull()
        {
            Assert.Equal("", User.NormalizePhoneNumber(null!));
            Assert.Equal("+989123456789", User.NormalizePhoneNumber(" +989123456789 "));
        }

        [Fact]
        public void Fullname_ShouldBeComputed_FromFirstNameAndLastName()
        {
            var user = User.CreateNew(
                "a@b.com",
                "ali",
                "Ali",
                "Rezaei",
                "+989123456789",
                "HASH",
                Now(),
                null);

            Assert.Equal("Ali Rezaei", user.Fullname);
        }

        [Fact]
        public void CreateNew_WhenEmailEmpty_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("  ", "ali", "Ali", "Rezaei", "+989123456789", "HASH", Now(), null));

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
                User.CreateNew(email, "ali", "Ali", "Rezaei", "+989123456789", "HASH", Now(), null));

            Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("format", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenEmailTooLong_ShouldThrow()
        {
            var email = $"{LongString(User.EmailMaxLength)}@a.com";

            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew(email, "ali", "Ali", "Rezaei", "+989123456789", "HASH", Now(), null));

            Assert.Contains("email", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("abcdefghijklmnopqrstuvwxyz")]
        public void CreateNew_WhenUsernameLengthInvalid_ShouldThrow(string username)
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", username, "Ali", "Rezaei", "+989123456789", "HASH", Now(), null));

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
                User.CreateNew("a@b.com", username, "Ali", "Rezaei", "+989123456789", "HASH", Now(), null));

            Assert.Contains("reserved", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("1ali")]
        [InlineData("_ali")]
        [InlineData(".ali")]
        [InlineData("ali-xx")]
        [InlineData("ali..xx")]
        [InlineData("ali.")]
        [InlineData("al i")]
        public void CreateNew_WhenUsernameFormatInvalid_ShouldThrow(string username)
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", username, "Ali", "Rezaei", "+989123456789", "HASH", Now(), null));

            Assert.Contains("username", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("invalid", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenFirstNameEmpty_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", "ali", " ", "Rezaei", "+989123456789", "HASH", Now(), null));

            Assert.Contains("FirstName", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenFirstNameTooLong_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew(
                    "a@b.com",
                    "ali",
                    LongString(User.FirstNameMaxLength + 1),
                    "Rezaei",
                    "+989123456789",
                    "HASH",
                    Now(),
                    null));

            Assert.Contains("FirstName", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenLastNameEmpty_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", "ali", "Ali", " ", "+989123456789", "HASH", Now(), null));

            Assert.Contains("LastName", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenLastNameTooLong_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew(
                    "a@b.com",
                    "ali",
                    "Ali",
                    LongString(User.LastNameMaxLength + 1),
                    "+989123456789",
                    "HASH",
                    Now(),
                    null));

            Assert.Contains("LastName", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenPhoneNumberEmpty_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", "ali", "Ali", "Rezaei", " ", "HASH", Now(), null));

            Assert.Contains("PhoneNumber", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("9123456789")]
        [InlineData("+9809123456789")]
        [InlineData("02112345678")]
        [InlineData("+98912abc678")]
        [InlineData("not-a-phone")]
        public void CreateNew_WhenPhoneNumberInvalid_ShouldThrow(string phoneNumber)
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", "ali", "Ali", "Rezaei", phoneNumber, "HASH", Now(), null));

            Assert.Contains("phone", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("invalid", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("+989123456789")]
        [InlineData("09123456789")]
        public void CreateNew_WhenPhoneNumberValid_ShouldCreateUser(string phoneNumber)
        {
            var user = User.CreateNew(
                "a@b.com",
                "ali",
                "Ali",
                "Rezaei",
                phoneNumber,
                "HASH",
                Now(),
                null);

            Assert.Equal(phoneNumber, user.PhoneNumber);
        }

        [Fact]
        public void CreateNew_WhenPasswordHashEmpty_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew("a@b.com", "ali", "Ali", "Rezaei", "+989123456789", " ", Now(), null));

            Assert.Contains("passwordHash", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_WhenPasswordHashTooLong_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() =>
                User.CreateNew(
                    "a@b.com",
                    "ali",
                    "Ali",
                    "Rezaei",
                    "+989123456789",
                    LongString(User.PasswordHashMaxLength + 1),
                    Now(),
                    null));

            Assert.Contains("passwordHash", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void CreateNew_ShouldNormalizeEmail_And_TrimFields_AndInitCollections()
        {
            var now = Now();

            var user = User.CreateNew(
                email: " TEST@Example.com ",
                username: " ali ",
                firstName: " Ali ",
                lastName: " Rezaei ",
                phoneNumber: " +989123456789 ",
                passwordHash: "HASH",
                nowUtc: now,
                actorUserId: null);

            Assert.Equal("test@example.com", user.Email);
            Assert.Equal("ali", user.Username);
            Assert.Equal("Ali", user.FirstName);
            Assert.Equal("Rezaei", user.LastName);
            Assert.Equal("+989123456789", user.PhoneNumber);
            Assert.Equal("Ali Rezaei", user.Fullname);
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
        public void ChangeProfile_ShouldTrim_AndUpdateAudit()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = CreateValidUser(t1);

            user.ChangeProfile(" Sara ", " Ahmadi ", " 09123456789 ", t2, user.Id);

            Assert.Equal("Sara", user.FirstName);
            Assert.Equal("Ahmadi", user.LastName);
            Assert.Equal("09123456789", user.PhoneNumber);
            Assert.Equal("Sara Ahmadi", user.Fullname);
            Assert.Equal(t2, user.UpdatedAt);
            Assert.Equal(user.Id, user.UpdatedByUserId);
        }

        [Fact]
        public void ChangeProfile_WhenFirstNameEmpty_ShouldThrow()
        {
            var user = CreateValidUser();

            var ex = Assert.Throws<DomainException>(() =>
                user.ChangeProfile(" ", "Ahmadi", "09123456789", Now().AddMinutes(1), user.Id));

            Assert.Contains("FirstName", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeProfile_WhenLastNameEmpty_ShouldThrow()
        {
            var user = CreateValidUser();

            var ex = Assert.Throws<DomainException>(() =>
                user.ChangeProfile("Sara", " ", "09123456789", Now().AddMinutes(1), user.Id));

            Assert.Contains("LastName", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeProfile_WhenPhoneNumberEmpty_ShouldThrow()
        {
            var user = CreateValidUser();

            var ex = Assert.Throws<DomainException>(() =>
                user.ChangeProfile("Sara", "Ahmadi", " ", Now().AddMinutes(1), user.Id));

            Assert.Contains("PhoneNumber", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("required", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeProfile_WhenPhoneNumberInvalid_ShouldThrow()
        {
            var user = CreateValidUser();

            var ex = Assert.Throws<DomainException>(() =>
                user.ChangeProfile("Sara", "Ahmadi", "12345", Now().AddMinutes(1), user.Id));

            Assert.Contains("phone", ex.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("invalid", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangeProfile_WhenSameValue_ShouldNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = CreateValidUser(t1);
            var before = user.UpdatedAt;

            user.ChangeProfile(" Ali ", " Rezaei ", " +989123456789 ", t2, user.Id);

            Assert.Equal("Ali", user.FirstName);
            Assert.Equal("Rezaei", user.LastName);
            Assert.Equal("+989123456789", user.PhoneNumber);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Fact]
        public void ChangePasswordHash_WhenEmpty_ShouldThrow()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);
            var user = CreateValidUser(t1);

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
            var user = CreateValidUser(t1);

            var ex = Assert.Throws<DomainException>(() =>
                user.ChangePasswordHash(LongString(User.PasswordHashMaxLength + 1), t2, user.Id));

            Assert.Contains("passwordHash", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ChangePasswordHash_ShouldUpdateHash_AndAudit()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = User.CreateNew(
                "a@b.com",
                "ali",
                "Ali",
                "Rezaei",
                "+989123456789",
                "HASH1",
                t1,
                null);

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

            var user = CreateValidUser(t1);
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

            var user = CreateValidUser(t1);
            Assert.True(user.IsActive);

            user.Deactivate(t2, user.Id);

            Assert.False(user.IsActive);
            Assert.Equal(t2, user.UpdatedAt);
        }

        [Fact]
        public void Deactive_WhenAlreadyInactive_ShouldNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);
            var t3 = t2.AddMinutes(1);

            var user = CreateValidUser(t1);

            user.Deactivate(t2, user.Id);
            var before = user.UpdatedAt;

            user.Deactivate(t3, user.Id);

            Assert.False(user.IsActive);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Fact]
        public void Active_ShouldSetIsActiveTrue_AndTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);
            var t3 = t2.AddMinutes(1);

            var user = CreateValidUser(t1);

            user.Deactivate(t2, user.Id);
            Assert.False(user.IsActive);

            user.Activate(t3, user.Id);

            Assert.True(user.IsActive);
            Assert.Equal(t3, user.UpdatedAt);
        }

        [Fact]
        public void Active_WhenAlreadyActive_ShouldNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);

            var user = CreateValidUser(t1);
            var before = user.UpdatedAt;

            user.Activate(t2, user.Id);

            Assert.True(user.IsActive);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Fact]
        public void AssignRole_WhenRoleIdInvalid_ShouldThrow()
        {
            var now = Now();
            var user = CreateValidUser(now);

            var ex = Assert.Throws<DomainException>(() =>
                user.AssignRole(0, now, user.Id));

            Assert.Contains("roleId", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void AssignRole_ShouldAddRole_AndTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);

            var user = CreateValidUser(t1);

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

            var user = CreateValidUser(t1);

            user.AssignRole(1, t2, user.Id);
            var before = user.UpdatedAt;

            user.AssignRole(1, t3, user.Id);

            Assert.Single(user.UserRoles);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Fact]
        public void RemoveRole_WhenRoleIdInvalid_ShouldThrowDomainException()
        {
            var now = Now();
            var user = CreateValidUser(now);

            var ex = Assert.Throws<DomainException>(() =>
                user.RemoveRole(0, now, user.Id));

            Assert.Equal("RoleId is invalid.", ex.Message, StringComparer.OrdinalIgnoreCase);
        }

        [Fact]
        public void RemoveRole_WhenRoleNotFound_ShouldNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(1);

            var user = CreateValidUser(t1);
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

            var user = CreateValidUser(t1);
            user.AssignRole(1, t2, user.Id);

            user.RemoveRole(1, t3, user.Id);

            Assert.Empty(user.UserRoles);
            Assert.Equal(t3, user.UpdatedAt);
        }

        [Fact]
        public void ChangeUsername_ShouldTrim_AndUpdateAudit()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = CreateValidUser(t1);

            user.ChangeUsername(" reza ", t2, user.Id);

            Assert.Equal("reza", user.Username);
            Assert.Equal(t2, user.UpdatedAt);
            Assert.Equal(user.Id, user.UpdatedByUserId);
        }

        [Fact]
        public void ChangeUsername_WhenSameValue_ShouldNotTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = CreateValidUser(t1);
            var before = user.UpdatedAt;

            user.ChangeUsername(" ali ", t2, Guid.NewGuid());

            Assert.Equal("ali", user.Username);
            Assert.Equal(before, user.UpdatedAt);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("ab")]
        [InlineData("1ali")]
        [InlineData("ali-xx")]
        [InlineData("ali..xx")]
        public void ChangeUsername_WhenInvalid_ShouldThrow(string username)
        {
            var user = CreateValidUser();

            var ex = Assert.Throws<DomainException>(() =>
                user.ChangeUsername(username, Now().AddMinutes(1), user.Id));

            Assert.Contains("username", ex.Message, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void SoftDelete_ShouldMarkDeleted_AndTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = CreateValidUser(t1);

            user.SoftDelete(t2, user.Id);

            Assert.True(user.IsDeleted);
            Assert.Equal(t2, user.DeletedAt);
            Assert.Equal(user.Id, user.DeletedByUserId);
            Assert.Equal(t2, user.UpdatedAt);
            Assert.Equal(user.Id, user.UpdatedByUserId);
        }

        [Fact]
        public void SoftDelete_WhenAlreadyDeleted_ShouldBeNoOp()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);
            var t3 = t2.AddMinutes(5);

            var user = CreateValidUser(t1);

            user.SoftDelete(t2, user.Id);

            var deletedAt = user.DeletedAt;
            var deletedBy = user.DeletedByUserId;
            var updatedAt = user.UpdatedAt;

            user.SoftDelete(t3, Guid.NewGuid());

            Assert.True(user.IsDeleted);
            Assert.Equal(deletedAt, user.DeletedAt);
            Assert.Equal(deletedBy, user.DeletedByUserId);
            Assert.Equal(updatedAt, user.UpdatedAt);
        }

        [Fact]
        public void Restore_ShouldClearDeletedFields_AndTouch()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);
            var t3 = t2.AddMinutes(5);

            var user = CreateValidUser(t1);

            user.SoftDelete(t2, user.Id);
            user.Restore(t3, user.Id);

            Assert.False(user.IsDeleted);
            Assert.Null(user.DeletedAt);
            Assert.Null(user.DeletedByUserId);
            Assert.Equal(t3, user.UpdatedAt);
            Assert.Equal(user.Id, user.UpdatedByUserId);
        }

        [Fact]
        public void Restore_WhenNotDeleted_ShouldBeNoOp()
        {
            var t1 = Now();
            var t2 = t1.AddMinutes(5);

            var user = CreateValidUser(t1);
            var before = user.UpdatedAt;

            user.Restore(t2, Guid.NewGuid());

            Assert.False(user.IsDeleted);
            Assert.Equal(before, user.UpdatedAt);
        }
    }
}