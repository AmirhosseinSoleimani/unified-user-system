using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using UnifiedUserSystem.src.Domain.Security.Entities;
using UnifiedUserSystem.src.Application.Abstractions.Security;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.UnifiedUserSystem.Infrastructure.Persistence;

namespace UnifiedUserSystem.UnitTests.Infrastructure.Persistence.Repositories.Security;

public class SecuritySettingsMappingTests
{
    [Fact]
    public void SecuritySettings_mapping_contains_expected_table_and_columns()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new AppDbContext(options, new TestCurrentUser(), new TestClock());

        var entityType = db.Model.FindEntityType(typeof(SecuritySettings));

        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("security_settings");
        entityType.FindProperty(nameof(SecuritySettings.IsMfaEnabled))!.GetColumnName().Should().Be("is_mfa_enabled");
        entityType.FindProperty(nameof(SecuritySettings.IsOtpEnabled))!.GetColumnName().Should().Be("is_otp_enabled");
        entityType.FindProperty(nameof(SecuritySettings.OtpExpirationMinutes))!.GetColumnName().Should().Be("otp_expiration_minutes");
        entityType.FindProperty(nameof(SecuritySettings.OtpMaxAttempts))!.GetColumnName().Should().Be("otp_max_attempts");
        entityType.FindProperty(nameof(SecuritySettings.LoginRateLimitPermitLimit))!.GetColumnName().Should().Be("login_rate_limit_permit_limit");
        entityType.FindProperty(nameof(SecuritySettings.LoginRateLimitWindowSeconds))!.GetColumnName().Should().Be("login_rate_limit_window_seconds");
        entityType.FindProperty(nameof(SecuritySettings.RefreshTokenRateLimitPermitLimit))!.GetColumnName().Should().Be("refresh_token_rate_limit_permit_limit");
        entityType.FindProperty(nameof(SecuritySettings.RefreshTokenRateLimitWindowSeconds))!.GetColumnName().Should().Be("refresh_token_rate_limit_window_seconds");
        entityType.FindProperty(nameof(SecuritySettings.AllowedIpRanges))!.GetColumnName().Should().Be("allowed_ip_ranges");
        entityType.FindProperty(nameof(SecuritySettings.BlockedIpRanges))!.GetColumnName().Should().Be("blocked_ip_ranges");
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId => null;
        public bool IsAuthenticated => false;
    }

    private sealed class TestClock : IClock
    {
        public DateTimeOffset Utcnow => DateTimeOffset.UnixEpoch;
    }
}