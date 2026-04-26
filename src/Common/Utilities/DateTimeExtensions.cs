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
    /// Converts DateTime to Unix timestamp (seconds since epoch)
    /// </summary>
    public static long ToUnixTimestamp(this DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts DateTimeOffset to Unix timestamp
    /// </summary>
    public static long ToUnixTimestamp(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToUnixTimeSeconds();
    }

    /// <summary>
    /// Converts Unix timestamp to DateTime
    /// </summary>
    public static DateTime FromUnixTimestamp(long timestamp)
    {
        return DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
    }

    /// <summary>
    /// Gets the start of the day (midnight)
    /// </summary>
    public static DateTime StartOfDay(this DateTime dateTime)
    {
        return dateTime.Date;
    }

    /// <summary>
    /// Gets the end of the day (23:59:59.999)
    /// </summary>
    public static DateTime EndOfDay(this DateTime dateTime)
    {
        return dateTime.Date.AddDays(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of the week (Monday)
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dateTime)
    {
        var daysToMonday = (dateTime.DayOfWeek - DayOfWeek.Monday + 7) % 7;
        return dateTime.AddDays(-daysToMonday).Date;
    }

    /// <summary>
    /// Gets the end of the week (Sunday)
    /// </summary>
    public static DateTime EndOfWeek(this DateTime dateTime)
    {
        return dateTime.StartOfWeek().AddDays(7).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of the month
    /// </summary>
    public static DateTime StartOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }

    /// <summary>
    /// Gets the end of the month
    /// </summary>
    public static DateTime EndOfMonth(this DateTime dateTime)
    {
        return dateTime.StartOfMonth().AddMonths(1).AddMilliseconds(-1);
    }

    /// <summary>
    /// Gets the start of the year
    /// </summary>
    public static DateTime StartOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 1, 1);
    }

    /// <summary>
    /// Gets the end of the year
    /// </summary>
    public static DateTime EndOfYear(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, 12, 31, 23, 59, 59, 999);
    }

    /// <summary>
    /// Checks if a date is in the past
    /// </summary>
    public static bool IsPast(this DateTime dateTime)
    {
        return dateTime < DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a date is in the future
    /// </summary>
    public static bool IsFuture(this DateTime dateTime)
    {
        return dateTime > DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a date is today
    /// </summary>
    public static bool IsToday(this DateTime dateTime)
    {
        return dateTime.Date == DateTime.UtcNow.Date;
    }

    /// <summary>
    /// Checks if a date is in the past week
    /// </summary>
    public static bool IsWithinPastDays(this DateTime dateTime, int days)
    {
        var threshold = DateTime.UtcNow.AddDays(-days);
        return dateTime >= threshold && dateTime <= DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a date is in the future within specified days
    /// </summary>
    public static bool IsWithinFutureDays(this DateTime dateTime, int days)
    {
        var threshold = DateTime.UtcNow.AddDays(days);
        return dateTime >= DateTime.UtcNow && dateTime <= threshold;
    }

    /// <summary>
    /// Gets a human-readable time difference
    /// </summary>
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
    public static string ToISO8601(this DateTime dateTime)
    {
        return dateTime.ToUniversalTime().ToString("O");
    }

    /// <summary>
    /// Formats a DateTimeOffset as ISO 8601 string
    /// </summary>
    public static string ToISO8601(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToUniversalTime().ToString("O");
    }

    /// <summary>
    /// Parses an ISO 8601 string to DateTime
    /// </summary>
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
    public static DateTime RoundToNearest(this DateTime dateTime, TimeSpan interval)
    {
        var totalTicks = (long)((dateTime.Ticks + interval.Ticks / 2) / interval.Ticks);
        return new DateTime(totalTicks * interval.Ticks);
    }

    /// <summary>
    /// Checks if two dates are on the same day
    /// </summary>
    public static bool IsSameDay(this DateTime dateTime1, DateTime dateTime2)
    {
        return dateTime1.Date == dateTime2.Date;
    }

    /// <summary>
    /// Gets the next occurrence of a specific day of week
    /// </summary>
    public static DateTime NextOccurrenceOf(this DateTime dateTime, DayOfWeek dayOfWeek)
    {
        var daysUntilDesired = ((int)dayOfWeek - (int)dateTime.DayOfWeek + 7) % 7;
        return dateTime.AddDays(daysUntilDesired == 0 ? 7 : daysUntilDesired);
    }
}
