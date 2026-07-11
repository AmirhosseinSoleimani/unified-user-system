using FluentAssertions;
using UnifiedUserSystem.src.Domain.Common;

using SecuritySettingsEntity =
    UnifiedUserSystem.src.Domain.Security.Entities.SecuritySettings;

namespace UnifiedUserSystem.UnitTests.Domain.Security.SecuritySettings;

[Trait("Category", "Domain")]
[Trait("Entity", "SecuritySettings")]
public sealed class SecuritySettingsIpRangeTests
{
    [Fact]
    public void Create_WithNullIpRanges_ShouldStoreEmptyValues()
    {
        var data = new SecuritySettingsTestData
        {
            AllowedIpRanges = null,
            BlockedIpRanges = null
        };

        var settings = data.Create();

        settings.AllowedIpRanges.Should().BeEmpty();
        settings.BlockedIpRanges.Should().BeEmpty();

        settings.GetAllowedIpRanges().Should().BeEmpty();
        settings.GetBlockedIpRanges().Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyIpRanges_ShouldStoreEmptyValues()
    {
        var data = new SecuritySettingsTestData
        {
            AllowedIpRanges = Array.Empty<string>(),
            BlockedIpRanges = Array.Empty<string>()
        };

        var settings = data.Create();

        settings.AllowedIpRanges.Should().BeEmpty();
        settings.BlockedIpRanges.Should().BeEmpty();
    }

    [Fact]
    public void Create_ShouldTrimAndIgnoreBlankIpRanges()
    {
        var data = new SecuritySettingsTestData
        {
            AllowedIpRanges =
            [
                " 10.0.0.0/8 ",
                "",
                " ",
                "192.168.1.10"
            ],
            BlockedIpRanges = Array.Empty<string>()
        };

        var settings = data.Create();

        settings.GetAllowedIpRanges()
            .Should()
            .Equal("10.0.0.0/8", "192.168.1.10");
    }

    [Fact]
    public void Create_ShouldRemoveDuplicateAllowedIpRanges()
    {
        var data = new SecuritySettingsTestData
        {
            AllowedIpRanges =
            [
                "10.0.0.0/8",
                "10.0.0.0/8",
                " 10.0.0.0/8 "
            ],
            BlockedIpRanges = Array.Empty<string>()
        };

        var settings = data.Create();

        settings.GetAllowedIpRanges()
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be("10.0.0.0/8");
    }

    [Fact]
    public void Create_ShouldRemoveDuplicateRangesIgnoringCase()
    {
        var data = new SecuritySettingsTestData
        {
            AllowedIpRanges =
            [
                "2001:DB8::/32",
                "2001:db8::/32"
            ],
            BlockedIpRanges = Array.Empty<string>()
        };

        var settings = data.Create();

        settings.GetAllowedIpRanges()
            .Should()
            .ContainSingle();
    }

    [Theory]
    [InlineData("192.168.1.10")]
    [InlineData("10.0.0.0/8")]
    [InlineData("0.0.0.0/0")]
    [InlineData("255.255.255.255/32")]
    [InlineData("2001:db8::1")]
    [InlineData("2001:db8::/32")]
    [InlineData("::/0")]
    [InlineData("::1/128")]
    public void Create_WithValidAllowedIpRange_ShouldCreateSettings(
        string range)
    {
        var data = new SecuritySettingsTestData
        {
            AllowedIpRanges = [range],
            BlockedIpRanges = Array.Empty<string>()
        };

        var settings = data.Create();

        settings.GetAllowedIpRanges().Should().ContainSingle(range);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("example.com")]
    [InlineData("192.168.1.256")]
    [InlineData("192.168.1.1/33")]
    [InlineData("2001:db8::1/129")]
    [InlineData("192.168.1.1/not-number")]
    [InlineData("/24")]
    public void Create_WithInvalidAllowedIpRange_ShouldThrow(
        string range)
    {
        var data = new SecuritySettingsTestData
        {
            AllowedIpRanges = [range],
            BlockedIpRanges = Array.Empty<string>()
        };

        var action = data.Create;

        action.Should()
            .Throw<DomainException>()
            .WithMessage($"*IP range '{range}' is invalid*");
    }

    [Fact]
    public void Create_WhenSameRangeExistsInAllowedAndBlocked_ShouldThrow()
    {
        var data = new SecuritySettingsTestData
        {
            AllowedIpRanges = ["10.0.0.0/8"],
            BlockedIpRanges = ["10.0.0.0/8"]
        };

        var action = data.Create;

        action.Should()
            .Throw<DomainException>()
            .WithMessage(
                "*AllowedIpRanges and BlockedIpRanges must not contain the same value*");
    }

    [Fact]
    public void Create_WhenConflictDiffersOnlyByCase_ShouldThrow()
    {
        var data = new SecuritySettingsTestData
        {
            AllowedIpRanges = ["2001:DB8::/32"],
            BlockedIpRanges = ["2001:db8::/32"]
        };

        var action = data.Create;

        action.Should()
            .Throw<DomainException>()
            .WithMessage(
                "*AllowedIpRanges and BlockedIpRanges must not contain the same value*");
    }

    [Fact]
    public void Create_WhenAllowedRangeStringExceedsMaximumLength_ShouldThrow()
    {
        var ranges = CreateIpRangesExceedingMaximumLength();

        var serializedLength = string.Join(",", ranges).Length;

        serializedLength.Should()
            .BeGreaterThan(SecuritySettingsEntity.IpRangesMaxLength);

        var data = new SecuritySettingsTestData
        {
            AllowedIpRanges = ranges,
            BlockedIpRanges = Array.Empty<string>()
        };

        var action = data.Create;

        action.Should()
            .Throw<DomainException>()
            .WithMessage("*AllowedIpRanges*");
    }

    [Fact]
    public void GetAllowedIpRanges_ShouldReturnIndependentArray()
    {
        var settings = new SecuritySettingsTestData
        {
            AllowedIpRanges = ["10.0.0.0/8"],
            BlockedIpRanges = Array.Empty<string>()
        }.Create();

        var firstResult = settings.GetAllowedIpRanges();

        firstResult[0] = "changed";

        var secondResult = settings.GetAllowedIpRanges();

        secondResult.Should().ContainSingle("10.0.0.0/8");
    }

    [Fact]
    public void GetBlockedIpRanges_ShouldReturnConfiguredValues()
    {
        var settings = new SecuritySettingsTestData
        {
            AllowedIpRanges = Array.Empty<string>(),
            BlockedIpRanges =
            [
                "203.0.113.0/24",
                "2001:db8::/32"
            ]
        }.Create();

        settings.GetBlockedIpRanges()
            .Should()
            .Equal(
                "203.0.113.0/24",
                "2001:db8::/32");
    }

    private static string[] CreateManyIpRanges()
    {
        return Enumerable
            .Range(0, 256)
            .Select(index =>
            {
                var secondOctet = index / 256;
                var thirdOctet = index % 256;

                return $"10.{secondOctet}.{thirdOctet}.1";
            })
            .ToArray();
    }


    private static string[] CreateIpRangesExceedingMaximumLength()
    {
        var ranges = new List<string>();
        var currentLength = 0;

        for (var secondOctet = 0; secondOctet <= 255; secondOctet++)
        {
            for (var thirdOctet = 0; thirdOctet <= 255; thirdOctet++)
            {
                var range = $"10.{secondOctet}.{thirdOctet}.1";

                var separatorLength = ranges.Count == 0 ? 0 : 1;

                ranges.Add(range);
                currentLength += range.Length + separatorLength;

                if (currentLength >
                    SecuritySettingsEntity.IpRangesMaxLength)
                {
                    return ranges.ToArray();
                }
            }
        }

        throw new InvalidOperationException(
            "Could not generate enough valid IP ranges.");
    }
}
