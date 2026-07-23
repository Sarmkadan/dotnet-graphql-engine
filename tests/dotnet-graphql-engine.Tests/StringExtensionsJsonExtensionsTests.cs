using System;
using GraphQLEngine.Common.Utilities;
using Xunit;

namespace GraphQLEngine.Tests
{
    public class StringExtensionsJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithNonEmptyString_ReturnsJsonString()
        {
            // Arrange
            var value = "hello";

            // Act
            var json = StringExtensionsJsonExtensions.ToJson(value);

            // Assert
            Assert.NotNull(json);
            Assert.Equal("\"hello\"", json);
            Assert.DoesNotContain("\n", json);
        }

        [Fact]
        public void ToJson_WithEmptyString_ReturnsEmptyJsonString()
        {
            // Arrange
            var value = string.Empty;

            // Act
            var json = StringExtensionsJsonExtensions.ToJson(value);

            // Assert
            Assert.Equal("\"\"", json);
        }

        [Fact]
        public void ToJson_WithWhitespaceString_ReturnsQuotedWhitespace()
        {
            // Arrange
            var value = "   ";

            // Act
            var json = StringExtensionsJsonExtensions.ToJson(value);

            // Assert
            Assert.Equal("\"   \"", json);
        }

        [Fact]
        public void ToJson_WithIndentation_ReturnsIndentedJson()
        {
            // Arrange
            var value = "indented";

            // Act
            var json = StringExtensionsJsonExtensions.ToJson(value, indented: true);

            // Assert
            Assert.Contains("\n", json);
        }

        [Fact]
        public void ToJson_NullValue_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => StringExtensionsJsonExtensions.ToJson(null!));
        }

        [Fact]
        public void FromJson_WithValidJson_ReturnsString()
        {
            // Arrange
            var json = "\"deserialized\"";

            // Act
            var result = StringExtensionsJsonExtensions.FromJson(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("deserialized", result);
        }

        [Fact]
        public void FromJson_WithEmptyString_ReturnsNull()
        {
            // Arrange
            var json = "";

            // Act
            var result = StringExtensionsJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_WithWhitespace_ReturnsNull()
        {
            // Arrange
            var json = "   ";

            // Act
            var result = StringExtensionsJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_WithInvalidJson_ReturnsNull()
        {
            // Arrange
            var json = "not a json";

            // Act
            var result = StringExtensionsJsonExtensions.FromJson(json);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void FromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => StringExtensionsJsonExtensions.FromJson(null!));
        }

        [Fact]
        public void TryFromJson_WithValidJson_ReturnsTrueAndValue()
        {
            // Arrange
            var json = "\"try\"";

            // Act
            var success = StringExtensionsJsonExtensions.TryFromJson(json, out var value);

            // Assert
            Assert.True(success);
            Assert.NotNull(value);
            Assert.Equal("try", value);
        }

        [Fact]
        public void TryFromJson_WithEmptyString_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "";

            // Act
            var success = StringExtensionsJsonExtensions.TryFromJson(json, out var value);

            // Assert
            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public void TryFromJson_WithWhitespace_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "   ";

            // Act
            var success = StringExtensionsJsonExtensions.TryFromJson(json, out var value);

            // Assert
            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "invalid json";

            // Act
            var success = StringExtensionsJsonExtensions.TryFromJson(json, out var value);

            // Assert
            Assert.False(success);
            Assert.Null(value);
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => StringExtensionsJsonExtensions.TryFromJson(null!, out _));
        }
    }
}
