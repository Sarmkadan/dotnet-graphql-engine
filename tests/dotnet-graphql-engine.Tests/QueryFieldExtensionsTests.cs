using System;
using System.Collections.Generic;
using FluentAssertions;
using GraphQLEngine.Domain.Entities;
using Xunit;

namespace GraphQLEngine.Tests
{
    public class QueryFieldExtensionsTests
    {
        [Fact]
        public void GetDisplayName_WithAlias_ReturnsAlias()
        {
            // Arrange
            var field = new QueryField("user", alias: "currentUser");

            // Act
            var displayName = field.GetDisplayName();

            // Assert
            displayName.Should().Be("currentUser");
        }

        [Fact]
        public void GetDisplayName_WithoutAlias_ReturnsName()
        {
            // Arrange
            var field = new QueryField("user");

            // Act
            var displayName = field.GetDisplayName();

            // Assert
            displayName.Should().Be("user");
        }

        [Fact]
        public void GetDisplayName_WithNullName_ThrowsArgumentNullException()
        {
            // Arrange
            var field = new QueryField(null!);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => field.GetDisplayName());
        }

        [Fact]
        public void GetDisplayName_WithEmptyNameAndNullAlias_ReturnsEmptyString()
        {
            // Arrange
            var field = new QueryField(string.Empty, alias: null);

            // Act
            var displayName = field.GetDisplayName();

            // Assert
            displayName.Should().BeEmpty();
        }

        [Fact]
        public void IsScalar_WithEmptyFieldsAndNoTypeCondition_ReturnsTrue()
        {
            // Arrange
            var field = new QueryField("id");

            // Act
            var isScalar = field.IsScalar();

            // Assert
            isScalar.Should().BeTrue();
        }

        [Fact]
        public void IsScalar_WithNestedFields_ReturnsFalse()
        {
            // Arrange
            var nestedField = new QueryField("name");
            var field = new QueryField("user", fields: new[] { nestedField });

            // Act
            var isScalar = field.IsScalar();

            // Assert
            isScalar.Should().BeFalse();
        }

        [Fact]
        public void IsScalar_WithTypeCondition_ReturnsFalse()
        {
            // Arrange
            var field = new QueryField("user", typeCondition: "User");

            // Act
            var isScalar = field.IsScalar();

            // Assert
            isScalar.Should().BeFalse();
        }

        [Fact]
        public void IsScalar_WithBothFieldsAndTypeCondition_ReturnsFalse()
        {
            // Arrange
            var nestedField = new QueryField("name");
            var field = new QueryField("user", typeCondition: "User", fields: new[] { nestedField });

            // Act
            var isScalar = field.IsScalar();

            // Assert
            isScalar.Should().BeFalse();
        }

        [Fact]
        public void IsScalar_WithNullField_ThrowsArgumentNullException()
        {
            // Arrange
            QueryField? field = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => field!.IsScalar());
        }

        [Fact]
        public void HasArguments_WithNoArguments_ReturnsFalse()
        {
            // Arrange
            var field = new QueryField("user");

            // Act
            var hasArguments = field.HasArguments();

            // Assert
            hasArguments.Should().BeFalse();
        }

        [Fact]
        public void HasArguments_WithArguments_ReturnsTrue()
        {
            // Arrange
            var arguments = new[] { new QueryArgument("id", 123) };
            var field = new QueryField("user", arguments: arguments);

            // Act
            var hasArguments = field.HasArguments();

            // Assert
            hasArguments.Should().BeTrue();
        }

        [Fact]
        public void HasArguments_WithEmptyArgumentsCollection_ReturnsFalse()
        {
            // Arrange
            var field = new QueryField("user", arguments: Array.Empty<QueryArgument>());

            // Act
            var hasArguments = field.HasArguments();

            // Assert
            hasArguments.Should().BeFalse();
        }

        [Fact]
        public void HasArguments_WithNullField_ThrowsArgumentNullException()
        {
            // Arrange
            QueryField? field = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => field!.HasArguments());
        }

        [Fact]
        public void HasArguments_WithNullArgumentsList_ReturnsFalse()
        {
            // Arrange
            var field = new QueryField("user", arguments: (IEnumerable<QueryArgument>?)null);

            // Act
            var hasArguments = field.HasArguments();

            // Assert
            hasArguments.Should().BeFalse();
        }

        [Fact]
        public void GetDisplayName_WithComplexAliasScenario_ReturnsCorrectDisplayName()
        {
            // Arrange
            var field = new QueryField("query", alias: "getUserData");

            // Act
            var displayName = field.GetDisplayName();

            // Assert
            displayName.Should().Be("getUserData");
        }

        [Fact]
        public void IsScalar_WithSingleNestedField_ReturnsFalse()
        {
            // Arrange
            var nestedField = new QueryField("name");
            var field = new QueryField("user", fields: new[] { nestedField });

            // Act
            var isScalar = field.IsScalar();

            // Assert
            isScalar.Should().BeFalse();
        }

        [Fact]
        public void HasArguments_WithMultipleArguments_ReturnsTrue()
        {
            // Arrange
            var arguments = new[]
            {
                new QueryArgument("id", 123),
                new QueryArgument("name", "John"),
                new QueryArgument("active", true)
            };
            var field = new QueryField("user", arguments: arguments);

            // Act
            var hasArguments = field.HasArguments();

            // Assert
            hasArguments.Should().BeTrue();
        }
    }
}
