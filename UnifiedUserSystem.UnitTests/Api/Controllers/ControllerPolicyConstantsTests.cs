using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using UnifiedUserSystem.src.Api.Controllers;
using UnifiedUserSystem.src.Application.Security;

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class ControllerPolicyConstantsTests
    {
        [Theory]
        [InlineData(typeof(UsersController), nameof(UsersController.GetActiveUsers), OperationPolicyNames.UsersRead)]
        [InlineData(typeof(UsersController), nameof(UsersController.UpdateUser), OperationPolicyNames.UsersUpdate)]
        [InlineData(typeof(RoleController), nameof(RoleController.List), OperationPolicyNames.RolesRead)]
        [InlineData(typeof(RoleController), nameof(RoleController.Create), OperationPolicyNames.RolesCreate)]
        [InlineData(typeof(OperationController), nameof(OperationController.List), OperationPolicyNames.OperationsRead)]
        [InlineData(typeof(OperationController), nameof(OperationController.Create), OperationPolicyNames.OperationsCreate)]
        public void Controllers_ShouldUseOperationPolicyConstants_NotRawPolicyStrings(Type controllerType, string methodName, string expectedPolicy)
        {
            var method = controllerType.GetMethod(methodName);

            var authorizeAttribute = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .Single();

            authorizeAttribute.Policy.Should().Be(expectedPolicy);
        }
    }
}