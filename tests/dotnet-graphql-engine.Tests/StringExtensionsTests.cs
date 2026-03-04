#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GraphQLEngine.Common.Utilities;
using Xunit;

namespace GraphQLEngine.Tests.Common.Utilities;

sealed public class StringExtensionsTests
{
    [Fact]
    public void ToCamelCase_WithUnderscoreDelimitedWords_ReturnsCamelCase()
    {
        // Arrange
        const string input = "user_profile_data";

        // Act
        var result = input.ToCamelCase();

        // Assert
        result.Should().Be("userProfileData");
    }

    [Fact]
    public void ToPascalCase_WithHyphenDelimitedWords_ReturnsPascalCase()
    {
        // Arrange
        const string input = "query-result-type";

        // Act
        var result = input.ToPascalCase();

        // Assert
        result.Should().Be("QueryResultType");
    }

    [Fact]
    public void IsValidGraphQLName_WhenFirstCharacterIsDigit_ReturnsFalse()
    {
        // Arrange
        const string input = "1InvalidFieldName";

        // Act
        var result = input.IsValidGraphQLName();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Truncate_WhenInputLengthExceedsMaximum_AppendsDefaultEllipsisSuffix()
    {
        // Arrange
        const string input = "Hello, GraphQL World!";
        const int maxLength = 10;

        // Act
        var result = input.Truncate(maxLength);

        // Assert
        result.Should().HaveLength(maxLength);
        result.Should().EndWith("...");
    }
}
