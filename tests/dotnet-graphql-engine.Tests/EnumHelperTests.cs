// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FluentAssertions;
using GraphQLEngine.Common.Utilities;

namespace GraphQLEngine.Tests;

public enum TestStatus { Active, Inactive, Pending }

public class EnumHelperTests
{
    [Fact]
    public void GetEnumValues_ReturnsAllValues()
    {
        var values = EnumHelper.GetEnumValues<TestStatus>();
        values.Should().HaveCount(3);
        values.Should().Contain(TestStatus.Active);
        values.Should().Contain(TestStatus.Inactive);
        values.Should().Contain(TestStatus.Pending);
    }

    [Fact]
    public void GetEnumNames_ReturnsAllNames()
    {
        var names = EnumHelper.GetEnumNames<TestStatus>();
        names.Should().HaveCount(3);
        names.Should().Contain("Active");
        names.Should().Contain("Inactive");
        names.Should().Contain("Pending");
    }

    [Fact]
    public void Parse_ValidName_ReturnsEnumValue()
    {
        var result = EnumHelper.Parse<TestStatus>("Active");
        result.Should().Be(TestStatus.Active);
    }

    [Fact]
    public void Parse_CaseInsensitive_ReturnsEnumValue()
    {
        var result = EnumHelper.Parse<TestStatus>("active");
        result.Should().Be(TestStatus.Active);
    }

    [Fact]
    public void Parse_NullValue_ReturnsDefault()
    {
        var result = EnumHelper.Parse<TestStatus>(null);
        result.Should().Be(default(TestStatus));
    }

    [Fact]
    public void Parse_EmptyValue_ReturnsDefault()
    {
        var result = EnumHelper.Parse<TestStatus>("");
        result.Should().Be(default(TestStatus));
    }

    [Fact]
    public void Parse_InvalidValue_ReturnsDefault()
    {
        var result = EnumHelper.Parse<TestStatus>("NonExistent");
        result.Should().Be(default(TestStatus));
    }

    [Fact]
    public void TryParse_ValidValue_ReturnsTrueAndValue()
    {
        var success = EnumHelper.TryParse<TestStatus>("Pending", out var result);
        success.Should().BeTrue();
        result.Should().Be(TestStatus.Pending);
    }

    [Fact]
    public void TryParse_NullValue_ReturnsFalse()
    {
        var success = EnumHelper.TryParse<TestStatus>(null, out _);
        success.Should().BeFalse();
    }

    [Fact]
    public void TryParse_InvalidValue_ReturnsFalse()
    {
        var success = EnumHelper.TryParse<TestStatus>("invalid", out _);
        success.Should().BeFalse();
    }
}
