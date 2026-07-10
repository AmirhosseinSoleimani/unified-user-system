using FluentAssertions;
using UnifiedUserSystem.src.Infrastructure.Security;

namespace UnifiedUserSystem.UnitTests.Security;

public sealed class OtpSecurityTests
{
    [Fact]
    public void OtpGenerator_Should_Generate_Numeric_Code_With_Configured_Length()
    {
        var generator = new OtpGenerator();

        var code = generator.Generate(6);

        code.Should().HaveLength(6);
        code.Should().MatchRegex("^\\d{6}$");
    }

    [Fact]
    public void OtpHasher_Should_Verify_Correct_Code()
    {
        var hasher = new Sha256OtpHasher();
        var challengeId = Guid.NewGuid();

        var hash = hasher.Hash("123456", challengeId);

        hasher.Verify("123456", challengeId, hash).Should().BeTrue();
        hasher.Verify("000000", challengeId, hash).Should().BeFalse();
    }

    [Fact]
    public void OtpHasher_Should_Bind_Hash_To_Challenge()
    {
        var hasher = new Sha256OtpHasher();
        var hash = hasher.Hash("123456", Guid.NewGuid());

        hasher.Verify("123456", Guid.NewGuid(), hash).Should().BeFalse();
    }
}
