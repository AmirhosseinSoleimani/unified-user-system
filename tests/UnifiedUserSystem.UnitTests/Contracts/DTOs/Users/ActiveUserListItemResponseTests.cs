using FluentAssertions;
using UnifiedUserSystem.src.Contracts.DTOs.Users;

namespace UnifiedUserSystem.UnitTests.Contracts.DTOs.Users
{
    public class ActiveUserListItemResponseTests
    {
        [Fact]
        public void ActiveUserListItemResponse_Should_NotExposePasswordOrPasswordHash()
        {
            // Act
            var propertyNames = typeof(ActiveUserListItemResponse)
                .GetProperties()
                .Select(x => x.Name)
                .ToArray();

            // Assert
            propertyNames.Should().NotContain("Password");
            propertyNames.Should().NotContain("PasswordHash");
        }
    }
}