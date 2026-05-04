using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Contracts.DTOs.Profile;

namespace UnifiedUserSystem.UnitTests.Contracts.DTOs.Profile
{
    public class ProfileResponseTests
    {
        [Fact]
        public void ProfileResponse_Should_NotExposePasswordOrPasswordHash()
        {
            // Act
            var propertyNames = typeof(ProfileResponse)
                .GetProperties()
                .Select(x => x.Name)
                .ToArray();

            // Assert
            propertyNames.Should().NotContain("Password");
            propertyNames.Should().NotContain("PasswordHash");
        }
    }
}
