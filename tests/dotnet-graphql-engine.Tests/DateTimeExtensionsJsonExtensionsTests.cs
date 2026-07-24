using System;
using System.Text.Json;
using GraphQLEngine.Common.Utilities;
using Xunit;

namespace GraphQLEngine.Tests
{
    public class DateTimeExtensionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_ValidDateTime_ReturnsJsonString()
        {
            // Arrange
            var dateTime = new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc);

            // Act
            var json = dateTime.ToJson();

            // Assert
            Assert.Contains("2026-07-26T12:00:00", json);
        }

        [Fact]
        public void ToJson_Indented_ReturnsJsonString()
        {
            // Arrange
            var dateTime = new DateTime(2026, 7, 26, 12, 0, 0, DateTimeKind.Utc);

            // Act
            var json = dateTime.ToJson(true);

            // Assert
            Assert.Contains("2026-07-26", json);
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsDateTime()
        {
            // Arrange
            var json = "\"2026-07-26T12:00:00Z\"";

            // Act
            var result = DateTimeExtensionsJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2026, result.Value.Year);
            Assert.Equal(7, result.Value.Month);
            Assert.Equal(26, result.Value.Day);
        }

        [Fact]
        public void FromJson_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => DateTimeExtensionsJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void FromJson_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => DateTimeExtensionsJsonExtensions.FromJson(""));
        }

        [Fact]
        public void FromJson_Whitespace_ThrowsJsonException()
        {
            // ArgumentException.ThrowIfNullOrEmpty doesn't throw for whitespace, so it proceeds to JsonSerializer.Deserialize which throws.
            Assert.Throws<JsonException>(() => DateTimeExtensionsJsonExtensions.FromJson("   "));
        }

        [Fact]
        public void FromJson_InvalidJsonFormat_ThrowsJsonException()
        {
            // Arrange
            var json = "\"invalid-date\"";

            // Act & Assert
            Assert.Throws<JsonException>(() => DateTimeExtensionsJsonExtensions.FromJson(json));
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndDateTime()
        {
            // Arrange
            var json = "\"2026-07-26T12:00:00Z\"";

            // Act
            var success = DateTimeExtensionsJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.True(success);
            Assert.NotNull(result);
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalse()
        {
            // Arrange
            var json = "\"invalid-date\"";

            // Act
            var success = DateTimeExtensionsJsonExtensions.TryFromJson(json, out var result);

            // Assert
            Assert.False(success);
            Assert.Null(result);
        }

        [Fact]
        public void TryFromJson_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => DateTimeExtensionsJsonExtensions.TryFromJson(null!, out _));
        }

        [Fact]
        public void TryFromJson_EmptyString_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => DateTimeExtensionsJsonExtensions.TryFromJson("", out _));
        }

        [Fact]
        public void TryFromJson_Whitespace_ReturnsFalse()
        {
            // ArgumentException.ThrowIfNullOrEmpty doesn't throw for whitespace, so it proceeds to JsonSerializer.Deserialize which catches JsonException and returns false.
            var success = DateTimeExtensionsJsonExtensions.TryFromJson("   ", out var result);
            Assert.False(success);
            Assert.Null(result);
        }
    }
}
