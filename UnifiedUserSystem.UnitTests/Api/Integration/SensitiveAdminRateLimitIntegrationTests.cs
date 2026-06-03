using FluentAssertions;
using Microsoft.AspNetCore.RateLimiting;
using UnifiedUserSystem.src.Api.Controllers;

namespace UnifiedUserSystem.UnitTests.Api.Integration
{
    public class SensitiveAdminRateLimitIntegrationTests
    {
        [Theory]
        [InlineData(typeof(UsersController), "UpdateUser")]
        [InlineData(typeof(UsersController), "DeactivateUser")]
        [InlineData(typeof(UsersController), "AssignRole")]
        [InlineData(typeof(UsersController), "RemoveRole")]
        [InlineData(typeof(UsersController), "ReplaceRoles")]
        [InlineData(typeof(RoleController), "List")]
        [InlineData(typeof(RoleController), "GetById")]
        [InlineData(typeof(RoleController), "Create")]
        [InlineData(typeof(RoleController), "Update")]
        [InlineData(typeof(RoleController), "Rename")]
        [InlineData(typeof(RoleController), "Delete")]
        [InlineData(typeof(RoleController), "Activate")]
        [InlineData(typeof(RoleController), "Deactivate")]
        [InlineData(typeof(RoleController), "Remove")]
        [InlineData(typeof(RoleController), "GetOperations")]
        [InlineData(typeof(RoleController), "AssignOperation")]
        [InlineData(typeof(RoleController), "RemoveOperation")]
        [InlineData(typeof(RoleController), "ReplaceOperations")]
        [InlineData(typeof(OperationController), "List")]
        [InlineData(typeof(OperationController), "GetById")]
        [InlineData(typeof(OperationController), "Create")]
        [InlineData(typeof(OperationController), "Update")]
        [InlineData(typeof(OperationController), "RenameTitle")]
        [InlineData(typeof(OperationController), "ChangeKey")]
        [InlineData(typeof(OperationController), "Delete")]
        [InlineData(typeof(OperationController), "Activate")]
        [InlineData(typeof(OperationController), "Deactivate")]
        public void SensitiveAdminEndpoint_ShouldUseSensitiveAdminRateLimitPolicy(
            Type controllerType,
            string actionName)
        {
            var method = controllerType.GetMethod(actionName);

            method.Should().NotBeNull($"{controllerType.Name}.{actionName} should exist");

            var attribute = method!
                .GetCustomAttributes(typeof(EnableRateLimitingAttribute), inherit: true)
                .OfType<EnableRateLimitingAttribute>()
                .SingleOrDefault();

            attribute.Should().NotBeNull($"{controllerType.Name}.{actionName} should be rate limited");
            attribute!.PolicyName.Should().Be("SensitiveAdminRateLimit");
        }
    }
}