using FluentAssertions;
using Microsoft.AspNetCore.RateLimiting;
using UnifiedUserSystem.src.Api.Controllers;

namespace UnifiedUserSystem.UnitTests.Api.Integration
{
    public class AuthControllerRateLimitIntegrationTests
    {
        [Theory]
        [InlineData(nameof(AuthController.Register))]
        [InlineData(nameof(AuthController.Login))]
        [InlineData(nameof(AuthController.Refresh))]
        [InlineData(nameof(AuthController.Logout))]
        [InlineData(nameof(AuthController.RevokeAllSessions))]
        public void AuthEndpoint_ShouldUseAuthRateLimitPolicy(string actionName)
        {
            var method = typeof(AuthController).GetMethod(actionName);

            method.Should().NotBeNull();

            var attribute = method!
                .GetCustomAttributes(typeof(EnableRateLimitingAttribute), inherit: true)
                .OfType<EnableRateLimitingAttribute>()
                .SingleOrDefault();

            attribute.Should().NotBeNull();
            attribute!.PolicyName.Should().Be("AuthRateLimit");
        }
    }
}