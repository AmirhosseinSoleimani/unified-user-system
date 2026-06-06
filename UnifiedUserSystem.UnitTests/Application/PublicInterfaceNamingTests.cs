using FluentAssertions;
using UnifiedUserSystem.src.Application.Interfaces.Services;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Application;

public class PublicInterfaceNamingTests
{
    [Fact]
    public void IOperationService_ShouldExposeActivateOperationAsync()
    {
        typeof(IOperationService)
            .GetMethod("ActivateOperationAsync")
            .Should()
            .NotBeNull();
    }

    [Fact]
    public void IOperationService_ShouldNotExposeActivateOperatioAsync()
    {
        typeof(IOperationService)
            .GetMethod("ActivateOperatioAsync")
            .Should()
            .BeNull();
    }

    [Fact]
    public void IOperationService_ShouldExposeDeactivateOperationAsync()
    {
        typeof(IOperationService)
            .GetMethod("DeactivateOperationAsync")
            .Should()
            .NotBeNull();
    }
}