using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Infrastructure.Security;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Security
{
    public class RefreshTokenServiceTests
    {
        [Fact]
        public void RefreshTokenService_GenerateToken_ShouldReturnCryptographicallyRandomBase64UrlToken()
        {
            var sut = CreateSut();

            var token = sut.GenerateToken();

            token.Should().NotBeNullOrWhiteSpace();
            token.Should().MatchRegex("^[A-Za-z0-9_-]+$");
        }

        [Fact]
        public void RefreshTokenService_GenerateToken_ShouldGenerateDifferentValues()
        {
            var sut = CreateSut();

            var tokens = Enumerable.Range(0, 50)
                .Select(_ => sut.GenerateToken())
                .ToArray();

            tokens.Distinct().Should().HaveCount(50);
        }

        [Fact]
        public void RefreshTokenService_GenerateToken_ShouldNotContainUnsafeBase64Characters()
        {
            var sut = CreateSut();

            var token = sut.GenerateToken();

            token.Should().NotContain("+");
            token.Should().NotContain("/");
            token.Should().NotContain("=");
        }

        [Fact]
        public void RefreshTokenService_HashToken_SameToken_ShouldReturnSameHash()
        {
            var sut = CreateSut();

            var first = sut.HashToken("refresh-token");
            var second = sut.HashToken("refresh-token");

            first.Should().Be(second);
        }

        [Fact]
        public void RefreshTokenService_HashToken_DifferentTokens_ShouldReturnDifferentHashes()
        {
            var sut = CreateSut();

            var first = sut.HashToken("refresh-token-1");
            var second = sut.HashToken("refresh-token-2");

            first.Should().NotBe(second);
        }

        [Fact]
        public void RefreshTokenService_HashToken_ShouldReturnSha256HexHash()
        {
            var sut = CreateSut();
            var token = "refresh-token";
            var expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)))
                .ToLowerInvariant();

            var hash = sut.HashToken(token);

            hash.Should().Be(expected);
            hash.Should().HaveLength(64);
            hash.Should().MatchRegex("^[a-f0-9]{64}$");
        }

        [Fact]
        public void RefreshTokenService_HashToken_WithNullToken_ShouldThrow()
        {
            var sut = CreateSut();

            Action act = () => sut.HashToken(null!);

            act.Should().Throw<Exception>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void RefreshTokenService_HashToken_WithEmptyToken_ShouldThrow(string token)
        {
            var sut = CreateSut();

            Action act = () => sut.HashToken(token);

            act.Should().Throw<Exception>();
        }

        [Fact]
        public void RefreshTokenService_GetExpiresAtUtc_ShouldUseConfiguredLifetime()
        {
            var sut = CreateSut(expiresDays: 14);
            var issuedAt = new DateTimeOffset(2026, 06, 01, 10, 00, 00, TimeSpan.Zero);

            var expiresAt = sut.GetExpiresAtUtc(issuedAt);

            expiresAt.Should().Be(issuedAt.AddDays(14));
        }

        [Fact]
        public void RefreshTokenService_GetExpiresAtUtc_ShouldUseUtcClock()
        {
            var sut = CreateSut(expiresDays: 7);
            var issuedAt = new DateTimeOffset(2026, 06, 01, 10, 00, 00, TimeSpan.Zero);

            var expiresAt = sut.GetExpiresAtUtc(issuedAt);

            expiresAt.Offset.Should().Be(TimeSpan.Zero);
        }

        [Fact]
        public void RefreshTokenService_GetExpiresAtUtc_WithInvalidLifetime_ShouldThrowOrFollowConfiguredValidation()
        {
            var sut = CreateSut(expiresDays: 0);

            Action act = () => sut.GetExpiresAtUtc(DateTimeOffset.UtcNow);

            act.Should().Throw<InvalidOperationException>();
        }

        private static RefreshTokenService CreateSut(int expiresDays = 7, int tokenSizeBytes = 64)
        {
            return new RefreshTokenService(Options.Create(new RefreshTokenOptions
            {
                ExpiresDays = expiresDays,
                TokenSizeBytes = tokenSizeBytes
            }));
        }
    }
}