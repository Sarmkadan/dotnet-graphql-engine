#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Xunit;

namespace GraphQLEngine.Tests.Common.Utilities;

using GraphQLEngine.Common.Utilities;

public sealed class EnumHelperValidationTests
{
    private enum TestEnum
    {
        [Display(Name = "First Item")]
        First = 0,
        Second = 1,
        Third = 2
    }

    [Flags]
    private enum TestFlags
    {
        None = 0,
        A = 1,
        B = 2
    }

    // EnumHelper.Parse<T>(null) returns default(T) rather than null, so
    // Validate() always flags "Parse<T>(null) should return null" for any
    // enum value/type. Tests below assert the actual current behavior.

    [Fact]
    public void Validate_ForAnyRegularEnumValue_AlwaysReportsParseNullProblem()
    {
        var problems = EnumHelperValidation.Validate(TestEnum.First);

        problems.Should().Contain("Parse<T>(null) should return null");
    }

    [Fact]
    public void Validate_ForAnyFlagsEnumValue_AlwaysReportsParseNullProblem()
    {
        var problems = EnumHelperValidation.Validate(TestFlags.A);

        problems.Should().Contain("Parse<T>(null) should return null");
    }

    [Fact]
    public void IsValid_ReturnsFalse_ForAnyEnumValue_BecauseOfParseNullDefect()
    {
        var result = EnumHelperValidation.IsValid(TestEnum.Second);

        result.Should().BeFalse();
    }

    [Fact]
    public void EnsureValid_Throws_ForAnyEnumValue_BecauseOfParseNullDefect()
    {
        Action act = () => EnumHelperValidation.EnsureValid(TestEnum.Third);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Parse<T>(null) should return null*");
    }

    [Fact]
    public void Validate_And_IsValid_AreConsistent_ForSameValue()
    {
        var problems = EnumHelperValidation.Validate(TestFlags.B);
        var isValid = EnumHelperValidation.IsValid(TestFlags.B);

        isValid.Should().Be(problems.Count == 0);
    }

    [Fact]
    public void Validate_DoesNotReportGetEnumValuesOrNamesProblems_ForKnownEnums()
    {
        var problems = EnumHelperValidation.Validate(TestEnum.First);

        problems.Should().NotContain(p => p.Contains("GetEnumValues"));
        problems.Should().NotContain(p => p.Contains("GetEnumNames"));
    }
}
