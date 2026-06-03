using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UnifiedUserSystem.src.Application.Options;

namespace UnifiedUserSystem.UnitTests.Application.Options
{
    public class AuthProtectionOptionsTests
    {
        [Fact]
        public void AuthProtectionOptions_Defaults_ShouldBeSecure()
        {
            var options = new AuthProtectionOptions();

            options.MaxFailedAttemptsPerIdentity.Should().BeLessThanOrEqualTo(5);
            options.MaxFailedAttemptsPerClient.Should().BeLessThanOrEqualTo(20);
            options.FailedAttemptWindowMinutes.Should().BeGreaterThan(0);
            options.LockoutMinutes.Should().BeGreaterThan(0);
            options.CooldownSeconds.Should().BeGreaterThanOrEqualTo(0);
        }

        [Fact]
        public void AuthProtectionOptions_WithInvalidThresholds_ShouldFailValidation()
        {
            var validator = new AuthProtectionOptionsValidator();

            var result = validator.Validate(
                Microsoft.Extensions.Options.Options.DefaultName,
                new AuthProtectionOptions
                {
                    MaxFailedAttemptsPerIdentity = 0,
                    MaxFailedAttemptsPerClient = 0,
                    FailedAttemptWindowMinutes = 15,
                    LockoutMinutes = 15,
                    CooldownSeconds = 2
                });

            result.Failed.Should().BeTrue();
        }

        [Fact]
        public void AuthProtectionOptions_WithInvalidDurations_ShouldFailValidation()
        {
            var validator = new AuthProtectionOptionsValidator();

            var result = validator.Validate(
                Microsoft.Extensions.Options.Options.DefaultName,
                new AuthProtectionOptions
                {
                    MaxFailedAttemptsPerIdentity = 5,
                    MaxFailedAttemptsPerClient = 20,
                    FailedAttemptWindowMinutes = 0,
                    LockoutMinutes = 0,
                    CooldownSeconds = -1
                });

            result.Failed.Should().BeTrue();
        }

        [Fact]
        public void AuthProtectionOptions_ShouldBindFromConfiguration()
        {
            var services = new ServiceCollection();

            services.AddOptions<AuthProtectionOptions>()
                .Configure(options =>
                {
                    options.MaxFailedAttemptsPerIdentity = 3;
                    options.MaxFailedAttemptsPerClient = 9;
                    options.FailedAttemptWindowMinutes = 4;
                    options.LockoutMinutes = 5;
                    options.CooldownSeconds = 1;
                });

            using var provider = services.BuildServiceProvider();

            var options = provider.GetRequiredService<IOptions<AuthProtectionOptions>>().Value;

            options.MaxFailedAttemptsPerIdentity.Should().Be(3);
            options.MaxFailedAttemptsPerClient.Should().Be(9);
            options.FailedAttemptWindowMinutes.Should().Be(4);
            options.LockoutMinutes.Should().Be(5);
            options.CooldownSeconds.Should().Be(1);
        }
    }
}