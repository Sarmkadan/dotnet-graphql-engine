#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GraphQLEngine.Common.Utilities;
using Xunit;

namespace GraphQLEngine.Tests.Common.Utilities;

sealed public class ValidationHelperTests
{
    [Fact]
    public void ValidateQueryString_WhenQueryIsEmpty_ReturnsFalseAndPopulatesErrorList()
    {
        // Arrange
        const string query = "";

        // Act
        var isValid = ValidationHelper.ValidateQueryString(query, out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().ContainSingle()
              .Which.Should().Contain("empty");
    }

    [Fact]
    public void ValidateQueryString_WithUnbalancedBraces_ReturnsFalseWithMismatchedBracesError()
    {
        // Arrange
        const string query = "{ users { id name }";  // opening brace count differs from closing

        // Act
        var isValid = ValidationHelper.ValidateQueryString(query, out var errors);

        // Assert
        isValid.Should().BeFalse();
        errors.Should().ContainSingle()
              .Which.Should().Contain("braces");
    }

    [Fact]
    public void ValidateEmail_WithWellFormedEmailAddress_ReturnsTrue()
    {
        // Arrange
        const string email = "admin@graphql.dev";

        // Act
        var result = ValidationHelper.ValidateEmail(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateComplexityScore_WhenScoreExceedsMaximum_ReturnsFalse()
    {
        // Arrange
        const int score = 5001;
        const int maxScore = 5000;

        // Act
        var result = ValidationHelper.ValidateComplexityScore(score, maxScore);

        // Assert
        result.Should().BeFalse();
    }
}
