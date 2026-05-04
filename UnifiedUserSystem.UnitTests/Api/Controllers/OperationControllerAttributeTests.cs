using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Api.Controllers;

namespace UnifiedUserSystem.UnitTests.Api.Controllers
{
    public class OperationControllerAttributeTests
    {
        [Fact]
        public void Activate_Should_HaveHttpPutAttributeWithOperationIdGuidActivateTemplate_When_ControllerAttributesAreInspected()
        {
            var method = typeof(OperationController).GetMethod(nameof(OperationController.Activate));

            method.Should().NotBeNull();

            var httpPutAttribute = method!
                .GetCustomAttributes(typeof(HttpPutAttribute), inherit: true)
                .Cast<HttpPutAttribute>()
                .SingleOrDefault();

            httpPutAttribute.Should().NotBeNull();
            httpPutAttribute!.Template.Should().Be("{operationId:guid}/activate");
        }

        [Fact]
        public void Activate_Should_HaveAuthorizeAttributeWithOperationActivatePolicy_When_ControllerAttributesAreInspected()
        {
            var method = typeof(OperationController).GetMethod(nameof(OperationController.Activate));

            method.Should().NotBeNull();

            var authorizeAttribute = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()
                .SingleOrDefault();

            authorizeAttribute.Should().NotBeNull();
            authorizeAttribute!.Policy.Should().Be("OP:operation.activate");
        }
    }
}
