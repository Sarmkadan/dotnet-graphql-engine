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
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is MinValue or MaxValue</exception>
    public static IReadOnlyList<string> Validate(this DateTime dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(dateTime, DateTime.MinValue, nameof(dateTime));
        ArgumentOutOfRangeException.ThrowIfEqual(dateTime, DateTime.MaxValue, nameof(dateTime));

        var problems = new List<string>();

        // Validate Unix timestamp methods (indirect check via reasonable range)
        // Unix timestamps should be reasonable (not too far in past/future)
        var unixTimestamp = dateTime.ToUnixTimestamp();
        var minReasonableTimestamp = new DateTime(2000, 1, 1).ToUnixTimestamp();
        var maxReasonableTimestamp = DateTime.UtcNow.AddYears(10).ToUnixTimestamp();

        if (unixTimestamp < minReasonableTimestamp || unixTimestamp > maxReasonableTimestamp)
        {
            problems.Add("Unix timestamp is outside reasonable range");
        }

        // Validate StartOfDay/EndOfDay - should be valid DateTime values
        var startOfDay = dateTime.StartOfDay();
        var endOfDay = dateTime.EndOfDay();

        if (startOfDay > endOfDay)
        {
            problems.Add("StartOfDay is after EndOfDay");
        }

        // Validate StartOfWeek/EndOfWeek - should be valid DateTime values
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

        // Validate StartOfMonth/EndOfMonth - should be valid DateTime values
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

        // Validate StartOfYear/EndOfYear - should be valid DateTime values
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

        // Validate IsPast/IsFuture/IsToday - should not throw exceptions
        _ = dateTime.IsPast();
        _ = dateTime.IsFuture();
        _ = dateTime.IsToday();

        // Validate IsWithinPastDays/IsWithinFutureDays - should handle valid day ranges
        _ = dateTime.IsWithinPastDays(7);
        _ = dateTime.IsWithinFutureDays(7);

        // Validate ToHumanReadableTime - should not throw exceptions
        _ = dateTime.ToHumanReadableTime();

        // Validate ToISO8601 - should not throw exceptions
        _ = dateTime.ToISO8601();

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Checks if a DateTime value is valid (has no validation problems)
    /// </summary>
    /// <param name="dateTime">The DateTime value to check</param>
    /// <returns>True if valid, false otherwise</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is MinValue or MaxValue</exception>
    public static bool IsValid(this DateTime dateTime)
        => Validate(dateTime).Count == 0;

    /// <summary>
    /// Ensures a DateTime value is valid, throwing ArgumentException if not
    /// </summary>
    /// <param name="dateTime">The DateTime value to validate</param>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when dateTime is MinValue or MaxValue</exception>
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