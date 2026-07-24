#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================


using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Xunit;

namespace GraphQLEngine.Tests.Common.Utilities;

using GraphQLEngine.Common.Utilities;

public sealed class EnumHelperTests
{
    // Test enum for basic functionality
    private enum TestEnum
    {
        [Display(Name = "First Item")]
        First = 0,

        [Display(Name = "Second Item")]
        [Description("This is the second item")]
        Second = 1,

        Third = 2,

        [Display(Name = "Fourth Item")]
        Fourth = 3
    }


    // Test enum for flags functionality
    [Flags]
    private enum TestFlags
    {
        None = 0,
        FlagA = 1,
        FlagB = 2,
        FlagC = 4,
        FlagD = 8
    }

    // Test enum for attribute functionality
    private enum TestAttributeEnum
    {
        [Description("Description for value A")]
        ValueA,

        [Description("Description for value B")]
        ValueB,

        ValueC
    }

    [Fact]
    public void GetEnumValues_WhenCalled_ReturnsAllEnumValues()
    {
        // Act
        var values = EnumHelper.GetEnumValues<TestEnum>();

        // Assert
        values.Should().HaveCount(4);
        values.Should().Contain(TestEnum.First);
        values.Should().Contain(TestEnum.Second);
        values.Should().Contain(TestEnum.Third);
        values.Should().Contain(TestEnum.Fourth);
        values.Should().BeInAscendingOrder(x => (int)x);
    }

    [Fact]
    public void GetEnumNames_WhenCalled_ReturnsAllEnumNames()
    {
        // Act
        var names = EnumHelper.GetEnumNames<TestEnum>();

        // Assert
        names.Should().HaveCount(4);
        names.Should().Contain("First");
        names.Should().Contain("Second");
        names.Should().Contain("Third");
        names.Should().Contain("Fourth");
    }

    [Fact]
    public void Parse_WithValidValue_ReturnsEnumValue()
    {
        // Act
        var result = EnumHelper.Parse<TestEnum>("Second");

        // Assert
        result.Should().Be(TestEnum.Second);
    }

    [Fact]
    public void Parse_WithCaseInsensitiveValue_ReturnsEnumValue()
    {
        // Act
        var result = EnumHelper.Parse<TestEnum>("sEcOnD");

        // Assert
        result.Should().Be(TestEnum.Second);
    }

    [Fact]
    public void Parse_WithNullValue_ReturnsDefault()
    {
        // Act
        var result = EnumHelper.Parse<TestEnum>(null);

        // Assert
        result.Should().Be(default(TestEnum));
    }

    [Fact]
    public void Parse_WithWhitespaceValue_ReturnsDefault()
    {
        // Act
        var result = EnumHelper.Parse<TestEnum>("   ");

        // Assert
        result.Should().Be(default(TestEnum));
    }

    [Fact]
    public void Parse_WithInvalidValue_ReturnsDefault()
    {
        // Act
        var result = EnumHelper.Parse<TestEnum>("InvalidValue");

        // Assert
        result.Should().Be(default(TestEnum));
    }

    [Fact]
    public void Parse_WithIgnoreCaseFalse_RespectsCase()
    {
        // Act
        var result = EnumHelper.Parse<TestEnum>("second", ignoreCase: false);

        // Assert
        result.Should().Be(default(TestEnum));
    }

    [Fact]
    public void TryParse_WithValidValue_ReturnsTrueAndSetsResult()
    {
        // Act
        var result = EnumHelper.TryParse<TestEnum>("Third", out var parsedValue);

        // Assert
        result.Should().BeTrue();
        parsedValue.Should().Be(TestEnum.Third);
    }

    [Fact]
    public void TryParse_WithNullValue_ReturnsFalseAndSetsDefault()
    {
        // Act
        var result = EnumHelper.TryParse<TestEnum>(null, out var parsedValue);

        // Assert
        result.Should().BeFalse();
        parsedValue.Should().Be(default);
    }

    [Fact]
    public void TryParse_WithInvalidValue_ReturnsFalseAndSetsDefault()
    {
        // Act
        var result = EnumHelper.TryParse<TestEnum>("Invalid", out var parsedValue);

        // Assert
        result.Should().BeFalse();
        parsedValue.Should().Be(default);
    }

    [Fact]
    public void GetDisplayName_WithDisplayAttribute_ReturnsDisplayName()
    {
        // Act
        var displayName = EnumHelper.GetDisplayName(TestEnum.First);

        // Assert
        displayName.Should().Be("First Item");
    }

    [Fact]
    public void GetDisplayName_WithoutDisplayAttribute_ReturnsEnumName()
    {
        // Act
        var displayName = EnumHelper.GetDisplayName(TestEnum.Third);

        // Assert
        displayName.Should().Be("Third");
    }

    [Fact]
    public void GetDescription_WithDescriptionAttribute_ReturnsDescription()
    {
        // Act
        var description = EnumHelper.GetDescription(TestEnum.Second);

        // Assert
        description.Should().Be("This is the second item");
    }

    [Fact]
    public void GetDescription_WithoutDescriptionAttribute_ReturnsNull()
    {
        // Act
        var description = EnumHelper.GetDescription(TestEnum.First);

        // Assert
        description.Should().BeNull();
    }

    [Fact]
    public void HasAttribute_WithExistingAttribute_ReturnsTrue()
    {
        // Act
        var hasAttribute = EnumHelper.HasAttribute<TestAttributeEnum, DescriptionAttribute>(TestAttributeEnum.ValueA);

        // Assert
        hasAttribute.Should().BeTrue();
    }

    [Fact]
    public void HasAttribute_WithoutAttribute_ReturnsFalse()
    {
        // Act
        var hasAttribute = EnumHelper.HasAttribute<TestAttributeEnum, DescriptionAttribute>(TestAttributeEnum.ValueC);

        // Assert
        hasAttribute.Should().BeFalse();
    }

    [Fact]
    public void GetAttributes_WithExistingAttributes_ReturnsAttributeList()
    {
        // Act
        var attributes = EnumHelper.GetAttributes<TestAttributeEnum, DescriptionAttribute>(TestAttributeEnum.ValueA);

        // Assert
        attributes.Should().HaveCount(1);
        attributes[0].Should().BeOfType<DescriptionAttribute>();
        attributes[0].Description.Should().Be("Description for value A");
    }

    [Fact]
    public void GetAttributes_WithoutAttributes_ReturnsEmptyList()
    {
        // Act
        var attributes = EnumHelper.GetAttributes<TestAttributeEnum, DescriptionAttribute>(TestAttributeEnum.ValueC);

        // Assert
        attributes.Should().BeEmpty();
    }

    [Fact]
    public void GetEnumDisplayDictionary_WhenCalled_ReturnsDictionaryWithDisplayNames()
    {
        // Act
        var dictionary = EnumHelper.GetEnumDisplayDictionary<TestEnum>();

        // Assert
        dictionary.Should().HaveCount(4);
        dictionary.Should().ContainKey("First").WhoseValue.Should().Be("First Item");
        dictionary.Should().ContainKey("Second").WhoseValue.Should().Be("Second Item");
        dictionary.Should().ContainKey("Third").WhoseValue.Should().Be("Third");
        dictionary.Should().ContainKey("Fourth").WhoseValue.Should().Be("Fourth Item");
    }

    [Fact]
    public void GetNextValue_WithMiddleValue_ReturnsNextValue()
    {
        // Act
        var nextValue = EnumHelper.GetNextValue(TestEnum.First);

        // Assert
        nextValue.Should().Be(TestEnum.Second);
    }

    [Fact]
    public void GetNextValue_WithLastValue_ReturnsDefault()
    {
        // Act
        var nextValue = EnumHelper.GetNextValue(TestEnum.Fourth);

        // Assert
        nextValue.Should().Be(default(TestEnum));
    }

    [Fact]
    public void GetPreviousValue_WithMiddleValue_ReturnsPreviousValue()
    {
        // Act
        var previousValue = EnumHelper.GetPreviousValue(TestEnum.Second);

        // Assert
        previousValue.Should().Be(TestEnum.First);
    }

    [Fact]
    public void GetPreviousValue_WithFirstValue_ReturnsDefault()
    {
        // Act
        var previousValue = EnumHelper.GetPreviousValue(TestEnum.First);

        // Assert
        previousValue.Should().Be(default(TestEnum));
    }

    [Fact]
    public void IsFlagsEnum_WithFlagsAttribute_ReturnsTrue()
    {
        // Act
        var isFlags = EnumHelper.IsFlagsEnum<TestFlags>();

        // Assert
        isFlags.Should().BeTrue();
    }

    [Fact]
    public void IsFlagsEnum_WithoutFlagsAttribute_ReturnsFalse()
    {
        // Act
        var isFlags = EnumHelper.IsFlagsEnum<TestEnum>();

        // Assert
        isFlags.Should().BeFalse();
    }

    [Fact]
    public void CombineFlags_WithMultipleFlags_ReturnsCombinedValue()
    {
        // Act
        var combined = EnumHelper.CombineFlags(TestFlags.FlagA, TestFlags.FlagC);

        // Assert
        combined.Should().Be(TestFlags.FlagA | TestFlags.FlagC);
    }

    [Fact]
    public void CombineFlags_WithSingleFlag_ReturnsSameValue()
    {
        // Act
        var combined = EnumHelper.CombineFlags(TestFlags.FlagB);

        // Assert
        combined.Should().Be(TestFlags.FlagB);
    }

    [Fact]
    public void CombineFlags_WithEmptyArray_ReturnsDefault()
    {
        // Act
        var combined = EnumHelper.CombineFlags<TestFlags>();

        // Assert
        combined.Should().Be(default(TestFlags));
    }

    [Fact]
    public void CombineFlags_WithNonFlagsEnum_ThrowsInvalidOperationException()
    {
        // Act
        Action act = () => EnumHelper.CombineFlags(TestEnum.First, TestEnum.Second);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void HasFlag_WithFlagSet_ReturnsTrue()
    {
        // Arrange
        var combined = TestFlags.FlagA | TestFlags.FlagC;

        // Act
        var hasFlag = EnumHelper.HasFlag(combined, TestFlags.FlagA);

        // Assert
        hasFlag.Should().BeTrue();
    }

    [Fact]
    public void HasFlag_WithFlagNotSet_ReturnsFalse()
    {
        // Arrange
        var combined = TestFlags.FlagA | TestFlags.FlagB;

        // Act
        var hasFlag = EnumHelper.HasFlag(combined, TestFlags.FlagC);

        // Assert
        hasFlag.Should().BeFalse();
    }

    [Fact]
    public void GetFlags_WithCombinedFlags_ReturnsIndividualFlags()
    {
        // Arrange
        var combined = TestFlags.FlagA | TestFlags.FlagC | TestFlags.FlagD;

        // Act
        var flags = EnumHelper.GetFlags(combined);

        // Assert
        flags.Should().HaveCount(3);
        flags.Should().Contain(TestFlags.FlagA);
        flags.Should().Contain(TestFlags.FlagC);
        flags.Should().Contain(TestFlags.FlagD);
    }

    [Fact]
    public void GetFlags_WithSingleFlag_ReturnsListWithOneFlag()
    {
        // Act
        var flags = EnumHelper.GetFlags(TestFlags.FlagB);

        // Assert
        flags.Should().HaveCount(1);
        flags.Should().Contain(TestFlags.FlagB);
    }

    [Fact]
    public void GetFlags_WithNoFlags_ReturnsEmptyList()
    {
        // Act
        var flags = EnumHelper.GetFlags(TestFlags.None);

        // Assert
        flags.Should().BeEmpty();
    }

    [Fact]
    public void GetUnderlyingValue_WithEnumValue_ReturnsUnderlyingValue()
    {
        // Act
        var underlyingValue = EnumHelper.GetUnderlyingValue(TestEnum.Second);

        // Assert
        underlyingValue.Should().Be(1);
        underlyingValue.Should().BeOfType<int>();
    }

    [Fact]
    public void IsValidEnumValue_WithValidValue_ReturnsTrue()
    {
        // Act
        var isValid = EnumHelper.IsValidEnumValue<TestEnum>("Second");

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValidEnumValue_WithCaseInsensitiveValidValue_ReturnsTrue()
    {
        // Act
        var isValid = EnumHelper.IsValidEnumValue<TestEnum>("sEcOnD");

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsValidEnumValue_WithInvalidValue_ReturnsFalse()
    {
        // Act
        var isValid = EnumHelper.IsValidEnumValue<TestEnum>("InvalidValue");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsValidEnumValue_WithEmptyString_ReturnsFalse()
    {
        // Act
        var isValid = EnumHelper.IsValidEnumValue<TestEnum>("");

        // Assert
        isValid.Should().BeFalse();
    }
}
