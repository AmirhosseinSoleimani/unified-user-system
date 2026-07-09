using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Infrastructure.Security;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Security;

public class RefreshTokenServiceTests
{
    [Fact]
    public void GenerateToken_Should_ReturnNonEmptyToken_When_OptionsAreValid()
    {
        var service = CreateService();

        var token = service.GenerateToken();

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public void GenerateToken_Should_ReturnDifferentTokens_When_CalledTwice()
    {
        var service = CreateService();

        var first = service.GenerateToken();
        var second = service.GenerateToken();

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void GenerateToken_Should_Throw_When_TokenSizeIsInvalid()
    {
        var service = CreateService(tokenSizeBytes: 0);

        var ex = Assert.Throws<InvalidOperationException>(() => service.GenerateToken());

        Assert.Contains("size", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GenerateToken_Should_Throw_When_ExpirationIsInvalid()
    {
        var service = CreateService(expiresDays: 0);

        var ex = Assert.Throws<InvalidOperationException>(() => service.GenerateToken());

        Assert.Contains("expiration", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HashToken_Should_ReturnDeterministicLowercaseSha256Hex()
    {
        var service = CreateService();

        var first = service.HashToken("token-value");
        var second = service.HashToken("token-value");

        Assert.Equal(first, second);
        Assert.Equal(64, first.Length);
        Assert.Equal(first.ToLowerInvariant(), first);
    }

    [Fact]
    public void HashToken_Should_Throw_When_TokenIsEmpty()
    {
        var service = CreateService();

        Assert.ThrowsAny<DomainException>(() => service.HashToken(" "));
    }

    [Fact]
    public void GetExpiresAtUtc_Should_AddConfiguredDays()
    {
        var issuedAt = new DateTimeOffset(2026, 02, 17, 10, 00, 00, TimeSpan.Zero);
        var service = CreateService(expiresDays: 14);

        var expiresAt = service.GetExpiresAtUtc(issuedAt);

        Assert.Equal(issuedAt.AddDays(14), expiresAt);
    }

    private static RefreshTokenService CreateService(int expiresDays = 7, int tokenSizeBytes = 32)
    {
        return new RefreshTokenService(Options.Create(new RefreshTokenOptions
        {
            ExpiresDays = expiresDays,
            TokenSizeBytes = tokenSizeBytes
        }));
    }
}