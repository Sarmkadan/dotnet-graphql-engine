#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Validation helpers for DateTime values to ensure they are valid before use
/// </summary>
public static class DateTimeExtensionsValidation
{
    /// <summary>
    /// Validates a DateTime value and returns a list of human-readable problems
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate</param>
    /// <returns>List of validation problems (empty if valid)</returns>
    public static IReadOnlyList<string> Validate(this DateTime dateTime)
    {
        var problems = new List<string>();

        // Check for default DateTime (MinValue) which indicates uninitialized/unspecified date
        if (dateTime == default)
        {
            problems.Add("DateTime value is default (uninitialized)");
        }

        // Validate Unix timestamp methods (indirect check via reasonable range)
        // Unix timestamps should be reasonable (not too far in past/future)
        try
        {
            var unixTimestamp = dateTime.ToUnixTimestamp();
            var minReasonableTimestamp = new DateTime(2000, 1, 1).ToUnixTimestamp();
            var maxReasonableTimestamp = DateTime.UtcNow.AddYears(10).ToUnixTimestamp();

            if (unixTimestamp < minReasonableTimestamp || unixTimestamp > maxReasonableTimestamp)
            {
                problems.Add("Unix timestamp is outside reasonable range");
            }
        }
        catch
        {
            problems.Add("Failed to convert to Unix timestamp");
        }

        // Validate StartOfDay/EndOfDay - should be valid DateTime values
        try
        {
            var startOfDay = dateTime.StartOfDay();
            var endOfDay = dateTime.EndOfDay();

            if (startOfDay > endOfDay)
            {
                problems.Add("StartOfDay is after EndOfDay");
            }
        }
        catch
        {
            problems.Add("Failed to calculate StartOfDay or EndOfDay");
        }

        // Validate StartOfWeek/EndOfWeek - should be valid DateTime values
        try
        {
            var startOfWeek = dateTime.StartOfWeek();
            var endOfWeek = dateTime.EndOfWeek();

            if (startOfWeek > endOfWeek)
            {
                problems.Add("StartOfWeek is after EndOfWeek");
            }

            // Week should be within reasonable bounds
            var minWeek = new DateTime(2000, 1, 1).StartOfWeek();
            var maxWeek = DateTime.UtcNow.AddYears(1).StartOfWeek();
            if (startOfWeek < minWeek || startOfWeek > maxWeek)
            {
                problems.Add("StartOfWeek is outside reasonable range");
            }
        }
        catch
        {
            problems.Add("Failed to calculate StartOfWeek or EndOfWeek");
        }

        // Validate StartOfMonth/EndOfMonth - should be valid DateTime values
        try
        {
            var startOfMonth = dateTime.StartOfMonth();
            var endOfMonth = dateTime.EndOfMonth();

            if (startOfMonth > endOfMonth)
            {
                problems.Add("StartOfMonth is after EndOfMonth");
            }

            // Month should be within reasonable bounds
            var minMonth = new DateTime(2000, 1, 1).StartOfMonth();
            var maxMonth = DateTime.UtcNow.AddYears(1).StartOfMonth();
            if (startOfMonth < minMonth || startOfMonth > maxMonth)
            {
                problems.Add("StartOfMonth is outside reasonable range");
            }
        }
        catch
        {
            problems.Add("Failed to calculate StartOfMonth or EndOfMonth");
        }

        // Validate StartOfYear/EndOfYear - should be valid DateTime values
        try
        {
            var startOfYear = dateTime.StartOfYear();
            var endOfYear = dateTime.EndOfYear();

            if (startOfYear > endOfYear)
            {
                problems.Add("StartOfYear is after EndOfYear");
            }

            // Year should be within reasonable bounds
            var minYear = new DateTime(2000, 1, 1).StartOfYear();
            var maxYear = DateTime.UtcNow.AddYears(10).StartOfYear();
            if (startOfYear.Year < 2000 || startOfYear.Year > maxYear.Year)
            {
                problems.Add("StartOfYear is outside reasonable range");
            }
        }
        catch
        {
            problems.Add("Failed to calculate StartOfYear or EndOfYear");
        }

        // Validate IsPast/IsFuture/IsToday - should not throw exceptions
        try
        {
            _ = dateTime.IsPast();
            _ = dateTime.IsFuture();
            _ = dateTime.IsToday();
        }
        catch
        {
            problems.Add("Failed to evaluate IsPast/IsFuture/IsToday");
        }

        // Validate IsWithinPastDays/IsWithinFutureDays - should handle valid day ranges
        try
        {
            _ = dateTime.IsWithinPastDays(7);
            _ = dateTime.IsWithinFutureDays(7);
        }
        catch
        {
            problems.Add("Failed to evaluate IsWithinPastDays or IsWithinFutureDays");
        }

        // Validate ToHumanReadableTime - should not throw exceptions
        try
        {
            _ = dateTime.ToHumanReadableTime();
        }
        catch
        {
            problems.Add("Failed to generate human readable time");
        }

        // Validate ToISO8601 - should not throw exceptions
        try
        {
            _ = dateTime.ToISO8601();
        }
        catch
        {
            problems.Add("Failed to format as ISO8601");
        }

        // Validate ParseISO8601 - should handle null/empty strings
        try
        {
            _ = DateTimeExtensions.ParseISO8601(null);
            _ = DateTimeExtensions.ParseISO8601(string.Empty);
            _ = DateTimeExtensions.ParseISO8601("invalid-date");
        }
        catch
        {
            problems.Add("ParseISO8601 failed to handle invalid input");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a DateTime value is valid (has no validation problems)
    /// </summary>
    /// <param name="dateTime">The DateTime value to check</param>
    /// <returns>True if valid, false otherwise</returns>
    public static bool IsValid(this DateTime dateTime)
    {
        return Validate(dateTime).Count == 0;
    }

    /// <summary>
    /// Ensures a DateTime value is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    public static void EnsureValid(this DateTime dateTime)
    {
        var problems = Validate(dateTime);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"DateTime validation failed:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", problems)}");
        }
    }
}