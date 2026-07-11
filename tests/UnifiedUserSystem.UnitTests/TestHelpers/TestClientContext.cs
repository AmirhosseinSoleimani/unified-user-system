using UnifiedUserSystem.src.Application.Abstractions.Web;

namespace UnifiedUserSystem.UnitTests.TestHelpers;

public sealed class TestClientContext : IClientContext
{
    public string? DeviceName { get; init; } = "test-device";
    public string? UserAgent { get; init; } = "test-agent";
    public string? IpAddress { get; init; } = "127.0.0.1";
    public string? ClientId { get; init; } = "test-client";
}