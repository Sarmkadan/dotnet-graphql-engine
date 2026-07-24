using System;
using System.Text.Json;
using FluentAssertions;
using GraphQLEngine.Common.Utilities;
using Xunit;

namespace GraphQLEngine.Tests
{
    // Simple enum used only for testing the JSON helpers
    public enum TestEnum
    {
        First = 0,
        Second = 1,
        Third = 2
    }

    public class EnumHelperJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithEnumValue_ReturnsJsonString()
        {
            // Arrange
            var value = TestEnum.Second;

            // Act
            var json = value.ToJson();

            // Assert
            json.Should().NotBeNullOrEmpty();
            // Default System.Text.Json serialises enums as numbers
            json.Should().Be("1");
        }

        [Fact]
        public void ToJson_WithIndentedOption_ProducesSameJsonForEnum()
        {
            // Arrange
            var value = TestEnum.Third;

            // Act
            var json = value.ToJson(indented: true);

            // Assert
            json.Should().Be("2");
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsEnumValue()
        {
            // Arrange
            var json = "0"; // JSON representation of TestEnum.First

            // Act
            var result = EnumHelperJsonExtensions.FromJson<TestEnum>(json);

            // Assert
            result.Should().Be(TestEnum.First);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string json = null!;

            // Act
            Action act = () => EnumHelperJsonExtensions.FromJson<TestEnum>(json);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_EmptyJson_ThrowsArgumentException()
        {
            // Arrange
            var json = string.Empty;

            // Act
            Action act = () => EnumHelperJsonExtensions.FromJson<TestEnum>(json);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void FromJson_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            var json = "not a json";

            // Act
            Action act = () => EnumHelperJsonExtensions.FromJson<TestEnum>(json);

            // Assert
            act.Should().Throw<JsonException>();
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndEnum()
        {
            // Arrange
            var json = "1"; // TestEnum.Second

            // Act
            var succeeded = EnumHelperJsonExtensions.TryFromJson<TestEnum>(json, out var value);

            // Assert
            succeeded.Should().BeTrue();
            value.Should().Be(TestEnum.Second);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "invalid json";

            // Act
            var succeeded = EnumHelperJsonExtensions.TryFromJson<TestEnum>(json, out var value);

            // Assert
            succeeded.Should().BeFalse();
            value.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string json = null!;

            // Act
            Action act = () => EnumHelperJsonExtensions.TryFromJson<TestEnum>(json, out _);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void TryFromJson_EmptyJson_ThrowsArgumentException()
        {
            // Arrange
            var json = string.Empty;

            // Act
            Action act = () => EnumHelperJsonExtensions.TryFromJson<TestEnum>(json, out _);

            // Assert
            act.Should().Throw<ArgumentException>();
        }
    }
}
