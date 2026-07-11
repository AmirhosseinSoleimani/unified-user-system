using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedUserSystem.src.Domain.Common;

namespace UnifiedUserSystem.UnitTests.Domain.Authorization.RoleOperations;

[Trait("Category", "Domain")]
[Trait("Feature", "Authorization")]
[Trait("Entity", "RoleOperation")]
public sealed class RoleOperationCreationTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateRoleOperation()
    {
        var roleOperation = RoleOperationTestFactory.Create();

        roleOperation.Id.Should().NotBeEmpty();

        roleOperation.RoleId.Should()
            .Be(RoleOperationTestFactory.RoleId);

        roleOperation.OperationId.Should()
            .Be(RoleOperationTestFactory.OperationId);

        roleOperation.IsDeleted.Should().BeFalse();
        roleOperation.DeletedAt.Should().BeNull();
        roleOperation.DeletedByUserId.Should().BeNull();
    }

    [Fact]
    public void Create_WithActor_ShouldInitializeAuditFields()
    {
        var roleOperation = RoleOperationTestFactory.Create(
            actorUserId: RoleOperationTestFactory.ActorUserId);

        roleOperation.CreatedAt.Should()
            .Be(RoleOperationTestFactory.CreatedAt);

        roleOperation.UpdatedAt.Should()
            .Be(RoleOperationTestFactory.CreatedAt);

        roleOperation.CreatedByUserId.Should()
            .Be(RoleOperationTestFactory.ActorUserId);

        roleOperation.UpdatedByUserId.Should()
            .Be(RoleOperationTestFactory.ActorUserId);
    }

    [Fact]
    public void Create_WithoutActor_ShouldKeepAuditActorNull()
    {
        var roleOperation = RoleOperationTestFactory.Create(
            actorUserId: null);

        roleOperation.CreatedByUserId.Should().BeNull();
        roleOperation.UpdatedByUserId.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldGenerateDifferentIds()
    {
        var first = RoleOperationTestFactory.Create();

        var second = RoleOperationTestFactory.Create(
            operationId: Guid.NewGuid());

        first.Id.Should().NotBe(second.Id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(int.MinValue)]
    public void Create_WhenRoleIdIsNotPositive_ShouldThrow(
        int roleId)
    {
        var action = () => RoleOperationTestFactory.Create(
            roleId: roleId);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*RoleId is invalid*");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(int.MaxValue)]
    public void Create_WhenRoleIdIsPositive_ShouldCreateEntity(
        int roleId)
    {
        var roleOperation = RoleOperationTestFactory.Create(
            roleId: roleId);

        roleOperation.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void Create_WhenOperationIdIsEmpty_ShouldThrow()
    {
        var action = () => RoleOperationTestFactory.Create(
            operationId: Guid.Empty);

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*OperationId is invalid*");
    }

    [Fact]
    public void Create_WithDifferentRolesAndSameOperation_ShouldCreateDifferentEntities()
    {
        var first = RoleOperationTestFactory.Create(
            roleId: 10);

        var second = RoleOperationTestFactory.Create(
            roleId: 20);

        first.Id.Should().NotBe(second.Id);

        first.RoleId.Should().Be(10);
        second.RoleId.Should().Be(20);

        first.OperationId.Should()
            .Be(second.OperationId);
    }

    [Fact]
    public void Create_WithSameRoleAndDifferentOperations_ShouldCreateDifferentEntities()
    {
        var firstOperationId = Guid.NewGuid();
        var secondOperationId = Guid.NewGuid();

        var first = RoleOperationTestFactory.Create(
            operationId: firstOperationId);

        var second = RoleOperationTestFactory.Create(
            operationId: secondOperationId);

        first.Id.Should().NotBe(second.Id);

        first.RoleId.Should().Be(second.RoleId);

        first.OperationId.Should().Be(firstOperationId);
        second.OperationId.Should().Be(secondOperationId);
    }

    [Fact]
    public void Create_WithExplicitCreationTime_ShouldUseProvidedTime()
    {
        var createdAt =
            RoleOperationTestFactory.CreatedAt.AddHours(2);

        var roleOperation = RoleOperationTestFactory.Create(
            nowUtc: createdAt);

        roleOperation.CreatedAt.Should().Be(createdAt);
        roleOperation.UpdatedAt.Should().Be(createdAt);
    }
}
