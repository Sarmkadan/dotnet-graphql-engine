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
        public void ToJson_WithStringType_ReturnsValidJsonWithTypeInfo()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var json = ReflectionHelperJsonExtensions.ToJson(type);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("System.String");
            json.Should().Contain("\"IsValueType\":false");
            json.Should().Contain("\"IsGenericType\":false");
        }

        [Fact]
        public void ToJson_WithIntType_ReturnsValidJsonWithTypeInfo()
        {
            // Arrange
            var type = typeof(int);

            // Act
            var json = ReflectionHelperJsonExtensions.ToJson(type);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("System.Int32");
            json.Should().Contain("\"IsValueType\":true");
            json.Should().Contain("\"IsGenericType\":false");
        }

        [Fact]
        public void ToJson_WithListType_ReturnsValidJsonWithGenericTypeInfo()
        {
            // Arrange
            var type = typeof(List<int>);

            // Act
            var json = ReflectionHelperJsonExtensions.ToJson(type);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\"IsGenericType\":true");
            json.Should().Contain("\"IsValueType\":false");
        }

        [Fact]
        public void ToJson_WithAbstractType_ReturnsValidJsonWithAbstractFlag()
        {
            // Arrange
            var type = typeof(System.IO.Stream);

            // Act
            var json = ReflectionHelperJsonExtensions.ToJson(type);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\"IsAbstract\":true");
        }

        [Fact]
        public void ToJson_WithIndentedTrue_ReturnsFormattedJson()
        {
            // Arrange
            var type = typeof(string);

            // Act
            var json = type.ToJson(indented: true);

            // Assert
            json.Should().NotBeNullOrEmpty();
            json.Should().Contain("\n");
            json.Should().Contain("TypeName");
            json.Should().Contain("System.String");
        }

        [Fact]
        public void ToJson_WithNullType_ThrowsArgumentNullException()
        {
            // Arrange
            Type type = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => type.ToJson());
        }

        [Fact]
        public void FromJson_WithValidStringTypeJson_ReturnsStringType()
        {
            // Arrange
            var json = "{\"TypeName\":\"System.String\",\"AssemblyQualifiedName\":\"System.String, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"IsGenericType\":false,\"IsAbstract\":false,\"IsValueType\":false}";

            // Act
            var type = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            type.Should().NotBeNull();
            type.Should().Be(typeof(string));
        }

        [Fact]
        public void FromJson_WithValidIntTypeJson_ReturnsIntType()
        {
            // Arrange
            var json = "{\"TypeName\":\"System.Int32\",\"AssemblyQualifiedName\":\"System.Int32, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"IsGenericType\":false,\"IsAbstract\":false,\"IsValueType\":true}";

            // Act
            var type = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            type.Should().NotBeNull();
            type.Should().Be(typeof(int));
        }

        [Fact]
        public void FromJson_WithNullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string json = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ReflectionHelperJsonExtensions.FromJson(json));
        }

        [Fact]
        public void FromJson_WithEmptyJson_ReturnsNull()
        {
            // Arrange
            var json = "";

            // Act
            var type = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            type.Should().BeNull();
        }

        [Fact]
        public void FromJson_WithWhitespaceJson_ReturnsNull()
        {
            // Arrange
            var json = "   \t\n  ";

            // Act
            var type = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            type.Should().BeNull();
        }

        [Fact]
        public void FromJson_WithInvalidJson_ReturnsNull()
        {
            // Arrange
            var json = "not a json";

            // Act
            var type = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            type.Should().BeNull();
        }

        [Fact]
        public void FromJson_WithNonExistentType_ReturnsNull()
        {
            // Arrange
            var json = "{\"TypeName\":\"NonExistent.Type\",\"AssemblyQualifiedName\":\"NonExistent.Type, Fake.Assembly, Version=1.0.0.0\",\"IsGenericType\":false,\"IsAbstract\":false,\"IsValueType\":false}";

            // Act
            var type = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            type.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_WithValidStringTypeJson_ReturnsTrueAndType()
        {
            // Arrange
            var json = "{\"TypeName\":\"System.String\",\"AssemblyQualifiedName\":\"System.String, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"IsGenericType\":false,\"IsAbstract\":false,\"IsValueType\":false}";

            // Act
            var result = ReflectionHelperJsonExtensions.TryFromJson(json, out var type);

            // Assert
            result.Should().BeTrue();
            type.Should().NotBeNull();
            type.Should().Be(typeof(string));
        }

        [Fact]
        public void TryFromJson_WithInvalidJson_ReturnsFalseAndNull()
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
        public void TryFromJson_WithEmptyJson_ReturnsFalseAndNull()
        {
            // Arrange
            var json = "";

            // Act
            var result = ReflectionHelperJsonExtensions.TryFromJson(json, out var type);

            // Assert
            result.Should().BeFalse();
            type.Should().BeNull();
        }

        [Fact]
        public void TryFromJson_WithNullJson_ThrowsArgumentNullException()
        {
            // Arrange
            string json = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => ReflectionHelperJsonExtensions.TryFromJson(json, out _));
        }

        [Fact]
        public void TypeNameProperty_FromToJson_RoundtripPreservesTypeName()
        {
            // Arrange
            var originalType = typeof(Dictionary<string, int>);

            // Act
            var json = originalType.ToJson();
            var deserializedType = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            deserializedType.Should().NotBeNull();
            deserializedType!.FullName.Should().Be(originalType.FullName);
        }

        [Fact]
        public void AssemblyQualifiedNameProperty_FromToJson_RoundtripPreservesAssemblyQualifiedName()
        {
            // Arrange
            var originalType = typeof(List<>);

            // Act
            var json = originalType.ToJson();
            var deserializedType = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            deserializedType.Should().NotBeNull();
            deserializedType!.AssemblyQualifiedName.Should().Be(originalType.AssemblyQualifiedName);
        }

        [Fact]
        public void IsGenericTypeProperty_FromToJson_RoundtripPreservesGenericFlag()
        {
            // Arrange
            var genericType = typeof(List<int>);
            var nonGenericType = typeof(string);

            // Act
            var genericJson = genericType.ToJson();
            var genericDeserialized = ReflectionHelperJsonExtensions.FromJson(genericJson);

            var nonGenericJson = nonGenericType.ToJson();
            var nonGenericDeserialized = ReflectionHelperJsonExtensions.FromJson(nonGenericJson);

            // Assert
            genericDeserialized.Should().NotBeNull();
            genericDeserialized!.IsGenericType.Should().BeTrue();

            nonGenericDeserialized.Should().NotBeNull();
            nonGenericDeserialized!.IsGenericType.Should().BeFalse();
        }

        [Fact]
        public void IsAbstractProperty_FromToJson_RoundtripPreservesAbstractFlag()
        {
            // Arrange
            var abstractType = typeof(System.IO.Stream);
            var concreteType = typeof(string);

            // Act
            var abstractJson = abstractType.ToJson();
            var abstractDeserialized = ReflectionHelperJsonExtensions.FromJson(abstractJson);

            var concreteJson = concreteType.ToJson();
            var concreteDeserialized = ReflectionHelperJsonExtensions.FromJson(concreteJson);

            // Assert
            abstractDeserialized.Should().NotBeNull();
            abstractDeserialized!.IsAbstract.Should().BeTrue();

            concreteDeserialized.Should().NotBeNull();
            concreteDeserialized!.IsAbstract.Should().BeFalse();
        }

        [Fact]
        public void IsValueTypeProperty_FromToJson_RoundtripPreservesValueTypeFlag()
        {
            // Arrange
            var valueType = typeof(int);
            var referenceType = typeof(string);

            // Act
            var valueTypeJson = valueType.ToJson();
            var valueTypeDeserialized = ReflectionHelperJsonExtensions.FromJson(valueTypeJson);

            var referenceTypeJson = referenceType.ToJson();
            var referenceTypeDeserialized = ReflectionHelperJsonExtensions.FromJson(referenceTypeJson);

            // Assert
            valueTypeDeserialized.Should().NotBeNull();
            valueTypeDeserialized!.IsValueType.Should().BeTrue();

            referenceTypeDeserialized.Should().NotBeNull();
            referenceTypeDeserialized!.IsValueType.Should().BeFalse();
        }

        [Fact]
        public void FromJson_WithGenericTypeDefinition_ReturnsGenericTypeDefinition()
        {
            // Arrange
            var json = "{\"TypeName\":\"System.Collections.Generic.List`1\",\"AssemblyQualifiedName\":\"System.Collections.Generic.List`1, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e\",\"IsGenericType\":true,\"IsAbstract\":false,\"IsValueType\":false}";

            // Act
            var type = ReflectionHelperJsonExtensions.FromJson(json);

            // Assert
            type.Should().NotBeNull();
            type.Should().Be(typeof(List<>));
            type!.IsGenericTypeDefinition.Should().BeTrue();
        }
    }
}