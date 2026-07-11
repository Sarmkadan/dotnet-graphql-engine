#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Validation utilities for <see cref="ValidationHelper"/> static class
/// </summary>
public static class ValidationHelperValidation
{
    /// <summary>
    /// Validates all public validation methods of <see cref="ValidationHelper"/> and returns human-readable problems
    /// </summary>
    /// <returns>List of validation problems, empty if valid</returns>
    /// <exception cref="ArgumentNullException">Thrown when any input parameter is null</exception>
    public static IReadOnlyList<string> Validate()
    {
        ArgumentNullException.ThrowIfNull(nameof(ValidationHelper));

        var errors = new List<string>();

        // Validate QueryString
        var queryResult = ValidationHelper.ValidateQueryString("query { test }", out var queryErrors);
        if (!queryResult)
        {
            errors.AddRange(queryErrors);
        }

        // Validate TypeName
        if (!ValidationHelper.ValidateTypeName("TestType"))
        {
            errors.Add("Type name validation failed for 'TestType'");
        }

        if (!ValidationHelper.ValidateTypeName(string.Empty))
        {
            errors.Add("Type name validation failed for empty string");
        }

        if (ValidationHelper.ValidateTypeName("123Invalid"))
        {
            errors.Add("Type name validation should fail for name starting with digit");
        }

        // Validate FieldName
        if (!ValidationHelper.ValidateFieldName("testField"))
        {
            errors.Add("Field name validation failed for 'testField'");
        }

        // Validate Email
        if (!ValidationHelper.ValidateEmail("test@example.com"))
        {
            errors.Add("Email validation failed for valid email 'test@example.com'");
        }

        if (ValidationHelper.ValidateEmail("invalid-email"))
        {
            errors.Add("Email validation should fail for invalid email 'invalid-email'");
        }

        if (ValidationHelper.ValidateEmail(string.Empty))
        {
            errors.Add("Email validation should fail for empty email");
        }

        // Validate URL
        if (!ValidationHelper.ValidateUrl("https://example.com"))
        {
            errors.Add("URL validation failed for valid URL 'https://example.com'");
        }

        if (ValidationHelper.ValidateUrl("not-a-url"))
        {
            errors.Add("URL validation should fail for invalid URL 'not-a-url'");
        }

        if (ValidationHelper.ValidateUrl(string.Empty))
        {
            errors.Add("URL validation should fail for empty URL");
        }

        // Validate ID
        if (!ValidationHelper.ValidateId("12345"))
        {
            errors.Add("ID validation failed for numeric ID '12345'");
        }

        if (!ValidationHelper.ValidateId("550e8400-e29b-41d4-a716-446655440000"))
        {
            errors.Add("ID validation failed for valid GUID");
        }

        if (ValidationHelper.ValidateId(string.Empty))
        {
            errors.Add("ID validation should fail for empty ID");
        }

        // Validate ComplexityScore - requires maxScore parameter
        if (!ValidationHelper.ValidateComplexityScore(5, 10))
        {
            errors.Add("Complexity score validation failed for valid score");
        }

        if (ValidationHelper.ValidateComplexityScore(-1, 10))
        {
            errors.Add("Complexity score validation should fail for negative score");
        }

        if (ValidationHelper.ValidateComplexityScore(11, 10))
        {
            errors.Add("Complexity score validation should fail for score exceeding max");
        }

        // Validate Depth - requires maxDepth parameter
        if (!ValidationHelper.ValidateDepth(3, 10))
        {
            errors.Add("Depth validation failed for valid depth");
        }

        if (ValidationHelper.ValidateDepth(-1, 10))
        {
            errors.Add("Depth validation should fail for negative depth");
        }

        if (ValidationHelper.ValidateDepth(11, 10))
        {
            errors.Add("Depth validation should fail for depth exceeding max");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the ValidationHelper is valid (all validation methods pass)
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid()
    {
        var errors = Validate();
        return errors.Count == 0;
    }

    /// <summary>
    /// Ensures the ValidationHelper is valid, throwing ArgumentException if not
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    public static void EnsureValid()
    {
        var errors = Validate();

        if (errors.Count > 0)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine("ValidationHelper validation failed:");

            for (int i = 0; i < errors.Count; i++)
            {
                errorMessage.AppendLine($" {i + 1}. {errors[i]}");
            }

            throw new ArgumentException(errorMessage.ToString());
        }
    }
}