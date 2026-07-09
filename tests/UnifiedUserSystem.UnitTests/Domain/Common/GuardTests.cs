using UnifiedUserSystem.src.Domain.Common;
using Xunit;

namespace UnifiedUserSystem.UnitTests.Domain.Common
{
    public class GuardTests
    {
        [Fact]
        public void NotNull_WhenValueIsNull_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() => Guard.NotNull(null, "value"));

            Assert.Equal("value is null", ex.Message);
        }

        [Fact]
        public void NotNull_WhenValueIsNotNull_ShouldNotThrow()
        {
            Guard.NotNull(new object(), "value");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void NotEmpty_WhenValueIsEmpty_ShouldThrow(string value)
        {
            var ex = Assert.Throws<DomainException>(() => Guard.NotEmpty(value, "name"));

            Assert.Equal("name is required.", ex.Message);
        }

        [Fact]
        public void NotEmpty_WhenValueHasWhitespace_ShouldReturnTrimmedValue()
        {
            var result = Guard.NotEmpty(" value ", "name");

            Assert.Equal("value", result);
        }

        [Fact]
        public void MaxLen_WhenValueExceedsMax_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() => Guard.MaxLen("abcd", 3, "name"));

            Assert.Equal("name max length is 3.", ex.Message);
        }

        [Fact]
        public void MaxLen_WhenValueIsAtMax_ShouldNotThrow()
        {
            Guard.MaxLen("abc", 3, "name");
        }

        [Fact]
        public void MinLen_WhenValueIsBelowMin_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() => Guard.MinLen("ab", 3, "name"));

            Assert.Equal("name min length is 3", ex.Message);
        }

        [Fact]
        public void MinLen_WhenValueIsAtMin_ShouldNotThrow()
        {
            Guard.MinLen("abc", 3, "name");
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("abcdef")]
        public void AllowedLen_WhenValueOutsideRange_ShouldThrow(string value)
        {
            var ex = Assert.Throws<DomainException>(() => Guard.AllowedLen(value, 5, 3, "name"));

            Assert.Equal("name must be between 3 and 5 characters.", ex.Message);
        }

        [Fact]
        public void AllowedLen_WhenValueInsideRange_ShouldNotThrow()
        {
            Guard.AllowedLen("abcd", 5, 3, "name");
        }

        [Fact]
        public void True_WhenConditionIsFalse_ShouldThrow()
        {
            var ex = Assert.Throws<DomainException>(() => Guard.True(false, "failed"));

            Assert.Equal("failed", ex.Message);
        }

        [Fact]
        public void True_WhenConditionIsTrue_ShouldNotThrow()
        {
            Guard.True(true, "failed");
        }
    }
}