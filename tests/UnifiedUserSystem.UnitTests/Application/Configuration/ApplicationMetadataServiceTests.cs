using FluentAssertions;
using Moq;
using UnifiedUserSystem.src.Application.Abstractions.Persistence;
using UnifiedUserSystem.src.Application.Abstractions.Services;
using UnifiedUserSystem.src.Application.Abstractions.Time;
using UnifiedUserSystem.src.Application.Models;
using UnifiedUserSystem.src.Application.Services.Configuration;
using UnifiedUserSystem.src.Contracts.DTOs.AppMetadata;
using UnifiedUserSystem.src.Domain.Common;
using UnifiedUserSystem.src.Domain.Configuration.Entities;
using UnifiedUserSystemsrc.src.Application.Options;

namespace UnifiedUserSystem.UnitTests.Application.Configuration;

public sealed class ApplicationMetadataServiceTests
{
    [Fact]
    public async Task GetBootstrapAsync_ShouldReturnErrorMessages_WhenClientVersionIsMissing()
    {
        var snapshot = new LocalizedMessageCacheSnapshot(
            Version: "v2",
            Messages: new Dictionary<string, LocalizedMessage>
            {
                ["bad_request.title"] = new("Bad request", "درخواست نامعتبر")
            });
        var service = CreateService(metadata: null, snapshot);

        var response = await service.GetBootstrapAsync(
            new AppBootstrapRequest
            {
                Language = "fa",
                Device = new AppDeviceRequest
                {
                    Platform = "Android",
                    DeviceId = "device-1",
                    DeviceModel = "Pixel",
                    OperatingSystem = "Android 15",
                    AppVersion = "1.0.0"
                }
            });

        response.Language.Should().Be("fa");
        response.Device.Platform.Should().Be("android");
        response.ErrorMessagesVersion.Should().Be("v2");
        response.ErrorMessagesChanged.Should().BeTrue();
        response.ErrorMessages.Should().ContainKey("bad_request.title");
        response.ErrorMessages["bad_request.title"].Selected.Should().Be("درخواست نامعتبر");
    }

    [Fact]
    public async Task GetBootstrapAsync_ShouldNotReturnErrorMessages_WhenClientVersionIsCurrent()
    {
        var snapshot = new LocalizedMessageCacheSnapshot(
            Version: "v2",
            Messages: new Dictionary<string, LocalizedMessage>
            {
                ["bad_request.title"] = new("Bad request", "درخواست نامعتبر")
            });
        var service = CreateService(metadata: null, snapshot);

        var response = await service.GetBootstrapAsync(
            new AppBootstrapRequest
            {
                Language = "en",
                ErrorMessagesVersion = "v2"
            });

        response.Language.Should().Be("en");
        response.ErrorMessagesVersion.Should().Be("v2");
        response.ErrorMessagesChanged.Should().BeFalse();
        response.ErrorMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBootstrapAsync_ShouldUseDatabaseMetadata_WhenItExists()
    {
        var now = new DateTimeOffset(2026, 7, 14, 0, 0, 0, TimeSpan.Zero);
        var metadata = ApplicationMetadata.CreateDefault(now, actorUserId: null);
        metadata.Update(
            latestWebVersion: "2.1.0",
            latestMobileVersion: "3.4.0",
            minimumSupportedWebVersion: "2.0.0",
            minimumSupportedMobileVersion: "3.0.0",
            webUpdateUrl: "https://example.com/web",
            androidUpdateUrl: "https://example.com/android",
            iosUpdateUrl: "https://example.com/ios",
            defaultLanguage: "fa",
            isMaintenanceMode: true,
            nowUtc: now,
            actorUserId: null);
        var snapshot = new LocalizedMessageCacheSnapshot("v1", new Dictionary<string, LocalizedMessage>());
        var service = CreateService(metadata, snapshot);

        var response = await service.GetBootstrapAsync(new AppBootstrapRequest());

        response.Language.Should().Be("fa");
        response.IsMaintenanceMode.Should().BeTrue();
        response.Versions.LatestWebVersion.Should().Be("2.1.0");
        response.Versions.LatestMobileVersion.Should().Be("3.4.0");
    }

    private static ApplicationMetadataService CreateService(
        ApplicationMetadata? metadata,
        LocalizedMessageCacheSnapshot snapshot)
    {
        var metadataRepository = new Mock<IApplicationMetadataRepository>();
        metadataRepository
            .Setup(x => x.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(metadata);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork
            .SetupGet(x => x.ApplicationMetadata)
            .Returns(metadataRepository.Object);

        var clock = new Mock<IClock>();
        clock.SetupGet(x => x.Utcnow).Returns(new DateTimeOffset(2026, 7, 14, 0, 0, 0, TimeSpan.Zero));

        var cache = new Mock<ILocalizedMessageCache>();
        cache.Setup(x => x.GetSnapshotAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);

        var defaults = Microsoft.Extensions.Options.Options.Create(new ApplicationMetadataDefaultsOptions
        {
            LatestWebVersion = "1.0.0",
            LatestMobileVersion = "1.0.0",
            MinimumSupportedWebVersion = "1.0.0",
            MinimumSupportedMobileVersion = "1.0.0",
            DefaultLanguage = "en"
        });

        return new ApplicationMetadataService(
            unitOfWork.Object,
            clock.Object,
            cache.Object,
            defaults);
    }
}

