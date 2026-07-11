
using FluentAssertions;
using Microsoft.AspNetCore.RateLimiting;
using UnifiedUserSystem.src.Api.Controllers;
using UnifiedUserSystem.src.Api.RateLimiting;

namespace UnifiedUserSystem.UnitTests.Api.Integration;

public class AuthControllerRateLimitIntegrationTests
{
    [Theory]
    [InlineData(nameof(AuthController.Register), SecurityRateLimitPolicies.Auth)]
    [InlineData(nameof(AuthController.Login), SecurityRateLimitPolicies.Auth)]
    [InlineData(nameof(AuthController.VerifyMfa), SecurityRateLimitPolicies.Auth)]
    [InlineData(nameof(AuthController.Refresh), SecurityRateLimitPolicies.Auth)]
    [InlineData(nameof(AuthController.Logout), SecurityRateLimitPolicies.Auth)]
    [InlineData(nameof(AuthController.RevokeAllSessions), SecurityRateLimitPolicies.Auth)]
    public void AuthEndpoint_ShouldUseSecurityRateLimitPolicy(
        string actionName,
        string expectedPolicyName)
    {
        var method = typeof(AuthController).GetMethod(actionName);

        method.Should().NotBeNull();

        var attribute = method!
            .GetCustomAttributes(typeof(EnableSecurityRateLimitingAttribute), inherit: true)
            .OfType<EnableSecurityRateLimitingAttribute>()
            .SingleOrDefault();

        attribute.Should().NotBeNull();
        attribute!.PolicyName.Should().Be(expectedPolicyName);
    }
}
