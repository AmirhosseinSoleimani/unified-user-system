using FluentAssertions;
using UnifiedUserSystem.src.Domain.Localization.Entities;

namespace UnifiedUserSystem.UnitTests.Domain.Localization;

public sealed class ErrorMessageTests
{
    [Fact]
    public void Create_ShouldNormalizeKeyAndSetTexts()
    {
        var now = new DateTimeOffset(2026, 7, 14, 0, 0, 0, TimeSpan.Zero);

        var message = ErrorMessage.Create(
            key: " Auth.Invalid_Credentials ",
            englishText: " Invalid username or password. ",
            persianText: " نام کاربری یا رمز عبور نامعتبر است. ",
            nowUtc: now,
            actorUserId: null);

        message.Key.Should().Be("auth.invalid_credentials");
        message.EnglishText.Should().Be("Invalid username or password.");
        message.PersianText.Should().Be("نام کاربری یا رمز عبور نامعتبر است.");
        message.IsActive.Should().BeTrue();
        message.CreatedAt.Should().Be(now);
        message.UpdatedAt.Should().Be(now);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("auth invalid")]
    [InlineData("auth/invalid")]
    [InlineData("auth:invalid")]
    public void Create_ShouldRejectInvalidKeys(string key)
    {
        var act = () => ErrorMessage.Create(
            key,
            "English",
            "فارسی",
            DateTimeOffset.UtcNow,
            actorUserId: null);

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Update_ShouldChangeTextsAndActiveState()
    {
        var createdAt = new DateTimeOffset(2026, 7, 14, 0, 0, 0, TimeSpan.Zero);
        var updatedAt = createdAt.AddMinutes(5);
        var message = ErrorMessage.Create(
            "server_error.detail",
            "Old English",
            "متن قدیمی",
            createdAt,
            actorUserId: null);

        message.Update(
            "server_error.detail",
            "New English",
            "متن جدید",
            isActive: false,
            updatedAt,
            actorUserId: null);

        message.EnglishText.Should().Be("New English");
        message.PersianText.Should().Be("متن جدید");
        message.IsActive.Should().BeFalse();
        message.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ToLocalizedMessage_ShouldReturnBothLanguages()
    {
        var message = ErrorMessage.Create(
            "bad_request.title",
            "Bad request",
            "درخواست نامعتبر",
            DateTimeOffset.UtcNow,
            actorUserId: null);

        var localized = message.ToLocalizedMessage();

        localized.English.Should().Be("Bad request");
        localized.Persian.Should().Be("درخواست نامعتبر");
        localized.Get("en").Should().Be("Bad request");
        localized.Get("fa").Should().Be("درخواست نامعتبر");
    }
}
