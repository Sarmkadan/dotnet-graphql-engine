#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Extension methods for DateTime and DateTimeOffset manipulation
/// Provides utilities for common date/time operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Converts DateTime to Unix timestamp (seconds since epoch, UTC)
    /// </summary>
    /// <param name="dateTime">The date and time to convert</param>
    /// <returns>Unix timestamp in seconds</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the resulting timestamp would overflow</exception>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return new DateTimeOffset(dateTime, TimeSpan.Zero).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts DateTimeOffset to Unix timestamp
    /// </summary>
    /// <param name="dateTimeOffset">The date and time to convert</param>
    /// <returns>Unix timestamp in seconds</returns>
    public static long ToUnixTimestamp(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime
    /// </summary>
    /// <param name="timestamp">Unix timestamp in seconds</param>
    /// <returns>DateTime representation of the timestamp</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the timestamp is outside the valid range</exception>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
    }

    /// <summary>
    /// Gets the start of the day (midnight)
    /// </summary>
    /// <param name="dateTime">The date and time</param>
    /// <returns>DateTime representing the start of the day</returns>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59.999)
    /// </summary>
    /// <param name="dateTime">The date and time</param>
    /// <returns>DateTime representing the end of the day</returns>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday)
    /// </summary>
    /// <param name="dateTime">The date and time</param>
    /// <returns>DateTime representing the start of the week (Monday)</returns>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        var daysToMonday = (dateTime.DayOfWeek - DayOfWeek.Monday + 7) % 7;
        return dateTime.AddDays(-daysToMonday).Date;
    }

    /// <summary>
    /// Gets the end of the week (Sunday)
    /// </summary>
    /// <param name="dateTime">The date and time</param>
    /// <returns>DateTime representing the end of the week (Sunday)</returns>
    public static DateTime EndOfWeek(this DateTime dateTime)
    {
        return dateTime.StartOfWeek().AddDays(7).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the month
    /// </summary>
    /// <param name="dateTime">The date and time</param>
    /// <returns>DateTime representing the start of the month</returns>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month
    /// </summary>
    /// <param name="dateTime">The date and time</param>
    /// <returns>DateTime representing the end of the month</returns>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddTicks(-1);
    }

    /// <summary>
    /// Gets the start of the year
    /// </summary>
    /// <param name="dateTime">The date and time</param>
    /// <returns>DateTime representing the start of the year</returns>
    public static DateTime StartOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Gets the end of the year
    /// </summary>
    /// <param name="dateTime">The date and time</param>
    /// <returns>DateTime representing the end of the year</returns>
    public static DateTime EndOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999);
    }

    /// <summary>
    /// Checks if a date is in the past
    /// </summary>
    /// <param name="dateTime">The date and time to check</param>
    /// <returns>True if the date is in the past; otherwise false</returns>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a date is in the future
    /// </summary>
    /// <param name="dateTime">The date and time to check</param>
    /// <returns>True if the date is in the future; otherwise false</returns>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a date is today
    /// </summary>
    /// <param name="dateTime">The date and time to check</param>
    /// <returns>True if the date is today; otherwise false</returns>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Checks if a date is in the past N days
    /// </summary>
    /// <param name="dateTime">The date and time to check</param>
    /// <param name="days">Number of days to check within</param>
    /// <returns>True if the date is within the past N days; otherwise false</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when days is negative</exception>
    public static bool IsWithinPastDays(this DateTime dateTime, int days)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(days);
        var threshold = DateTime.UtcNow.AddDays(-days);
        return dateTime >= threshold && dateTime <= DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a date is in the future within specified days
    /// </summary>
    /// <param name="dateTime">The date and time to check</param>
    /// <param name="days">Number of days to check within</param>
    /// <returns>True if the date is within the future N days; otherwise false</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when days is negative</exception>
    public static bool IsWithinFutureDays(this DateTime dateTime, int days)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(days);
        var threshold = DateTime.UtcNow.AddDays(days);
        return dateTime >= DateTime.UtcNow && dateTime <= threshold;
    }

    /// <summary>
    /// Gets a human-readable time difference
    /// </summary>
    /// <param name="dateTime">The date and time to compare</param>
    /// <returns>Human-readable string representing the time difference</returns>
    public static string ToHumanReadableTime(this DateTime dateTime)
    {
        var now = DateTime.UtcNow;
        var span = now - dateTime;

        return span.TotalSeconds switch
        {
            < 60 => "just now",
            < 120 => "1 minute ago",
            < 3600 => $"{(int)span.TotalMinutes} minutes ago",
            < 7200 => "1 hour ago",
            < 86400 => $"{(int)span.TotalHours} hours ago",
            < 172800 => "1 day ago",
            < 604800 => $"{(int)span.TotalDays} days ago",
            < 2592000 => $"{(int)(span.TotalDays / 7)} weeks ago",
            < 31536000 => $"{(int)(span.TotalDays / 30)} months ago",
            _ => $"{(int)(span.TotalDays / 365)} years ago"
        };
    }

    /// <summary>
    /// Formats a DateTime as ISO 8601 string
    /// </summary>
    /// <param name="dateTime">The date and time to format</param>
    /// <returns>ISO 8601 formatted string</returns>
    public static string ToISO8601(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("O");
    }

    /// <summary>
    /// Formats a DateTimeOffset as ISO 8601 string
    /// </summary>
    /// <param name="dateTimeOffset">The date and time to format</param>
    /// <returns>ISO 8601 formatted string</returns>
    public static string ToISO8601(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToUniversalTime().ToString("O");
    }

    /// <summary>
    /// Parses an ISO 8601 string to DateTime
    /// </summary>
    /// <param name="dateString">ISO 8601 formatted date string</param>
    /// <returns>Parsed DateTime or null if parsing fails</returns>
    public static DateTime? ParseISO8601(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString))
            return null;

        if (DateTime.TryParse(dateString, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind, out var result))
        {
            return result;
        }

        return null;
    }

    /// <summary>
    /// Gets the age in years between a date and now
    /// </summary>
    /// <param name="birthDate">The birth date</param>
    /// <returns>Age in years</returns>
    public static int GetAge(this DateTime birthDate)
    {
        var now = DateTime.UtcNow;
        var age = now.Year - birthDate.Year;

        if (birthDate.Date > now.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// Rounds a DateTime to the nearest interval
    /// </summary>
    /// <param name="dateTime">The date and time to round</param>
    /// <param name="interval">The time interval to round to</param>
    /// <returns>Rounded DateTime</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when interval is zero or negative</exception>
    public static DateTime RoundToNearest(this DateTime dateTime, TimeSpan interval)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);
        var totalTicks = (long)((dateTime.Ticks + interval.Ticks / 2) / interval.Ticks);
        return new DateTime(totalTicks * interval.Ticks);
    }

    /// <summary>
    /// Checks if two dates are on the same day
    /// </summary>
    /// <param name="dateTime1">First date to compare</param>
    /// <param name="dateTime2">Second date to compare</param>
    /// <returns>True if both dates are on the same day; otherwise false</returns>
    public static bool IsSameDay(this DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime1.Date == dateTime2.Date;
    }

    /// <summary>
    /// Gets the next occurrence of a specific day of week
    /// </summary>
    /// <param name="dateTime">The starting date</param>
    /// <param name="dayOfWeek">The target day of week</param>
    /// <returns>DateTime representing the next occurrence</returns>
    public static DateTime NextOccurrenceOf(this DateTime dateTime, DayOfWeek dayOfWeek)
    {
        var daysUntilDesired = ((int)dayOfWeek - (int)dateTime.DayOfWeek + 7) % 7;
        return dateTime.AddDays(daysUntilDesired == 0 ? 7 : daysUntilDesired);
    }
}