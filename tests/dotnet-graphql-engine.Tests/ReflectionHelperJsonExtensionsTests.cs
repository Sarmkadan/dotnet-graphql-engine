using System;
using System.Collections.Generic;
using FluentAssertions;
using GraphQLEngine.Common.Utilities;
using Xunit;

namespace GraphQLEngine.Tests
{
    public class ReflectionHelperJsonExtensionsTests
    {
        [Fact]
        public void ToJson_WithNonGenericReferenceType_ReturnsJsonContainingExpectedProperties()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var json = type.ToJson();

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\"typeName\":\"System.String\"");
            json.Should().Contain("\"assemblyQualifiedName\":\"System.String");
            json.Should().Contain("\"isGenericType\":false");
            json.Should().Contain("\"isAbstract\":false");
            json.Should().Contain("\"isValueType\":false");
        }

        [Fact]
        public void ToJson_WithIndentedOption_ProducesFormattedJson()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var json = type.ToJson(indented: true);

            // Assert
            json.Should().Contain("\n"); // formatted JSON contains line breaks
            json.Should().Contain("\"isGenericType\":true");
        }

        [Fact]
        public void ToJson_NullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null!;

            // Act
            Action act = () => type.ToJson();

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void FromJson_ValidJson_ReturnsCorrectType()
        {
            // Arrange
            var json = "{\"typeName\":\"System.Int32\",\"assemblyQualifiedName\":\"System.Int32, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"isGenericType\":false,\"isAbstract\":false,\"isValueType\":true}";

            // Act
            var type = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            type.Should().Be(typeof(int));
        }

        [Fact]
        public void FromJson_NullOrWhiteSpace_ReturnsNull()
        {
            // Arrange
            string nullJson = null!;
            string emptyJson = "";
            string whitespaceJson = "   \t\n  ";

            // Act & Assert
            ReflectionHelperJsonExtensions.FromJson(nullJson).Should().BeNull();
            ReflectionHelperJsonExtensions.FromJson(emptyJson).Should().BeNull();
            ReflectionHelperJsonExtensions.FromJson(whitespaceJson).Should().BeNull();
        }

        [Fact]
        public void FromJson_InvalidJson_ReturnsNull()
        {
            // Arrange
            var json = "not a json";

            // Act
            var type = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            type.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_ValidJson_ReturnsTrueAndType()
        {
            // Arrange
            var json = "{\"typeName\":\"System.String\",\"assemblyQualifiedName\":\"System.String, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"isGenericType\":false,\"isAbstract\":false,\"isValueType\":false}";

            // Act
            var result = ReflectionHelperJsonExtensions.TryFromJson(json, out var type);

            // Assert
            result.Should().BeTrue();
            type.Should().Be(typeof(string));
        }

        [Fact]
        public void TryFromJson_InvalidJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "invalid json";

            // Act
            var result = ReflectionHelperJsonExtensions.TryFromJson(json, out var type);

            // Assert
            result.Should().BeFalse();
            type.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_NullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string json = null!;

            // Act
            Action act = () => ReflectionHelperJsonExtensions.TryFromJson(json, out _);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
