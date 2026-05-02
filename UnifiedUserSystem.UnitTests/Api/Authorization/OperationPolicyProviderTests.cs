using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Api.Authorization;
using UnifiedUserSystem.src.Api.Controllers;

namespace UnifiedUserSystem.UnitTests.Api.Authorization
{
    public class OperationPolicyProviderTests
    {
        private static OperationPolicyProvider CreateSut()
        {
            var options = Options.Create(new AuthorizationOptions());
            return new OperationPolicyProvider(options);
        }

        [Fact]
        public async Task GetPolicyAsync_Should_CreateAuthorizationPolicyWithSingleOperationRequirement_When_PolicyUsesOpColonPrefix()
        {
            var sut = CreateSut();

            var policy = await sut.GetPolicyAsync("OP:role.create");

            policy.Should().NotBeNull();
            policy!.Requirements.Should().ContainSingle();

            var requirement = policy.Requirements.Single().Should().BeOfType<OperationRequirement>().Subject;
            requirement.OperationKey.Should().Be("role.create");
        }

        [Fact]
        public async Task GetPolicyAsync_Should_ReturnNull_When_PolicyUsesLegacyDotPrefix()
        {
            var sut = CreateSut();

            var policy = await sut.GetPolicyAsync("OP.operation.create");

            policy.Should().BeNull();
        }

        [Fact]
        public void ChangeKey_Should_UseOpColonOperationChangeKeyPolicy_When_AuthorizeAttributeIsApplied()
        {
            var method = typeof(OperationController).GetMethod(nameof(OperationController.ChangeKey));

            method.Should().NotBeNull();

            var authorizeAttribute = method!
                .GetCustomAttributes<AuthorizeAttribute>(inherit: true)
                .SingleOrDefault();

            authorizeAttribute.Should().NotBeNull();
            authorizeAttribute!.Policy.Should().Be("OP:operation.changeKey");
        }
    }
}
