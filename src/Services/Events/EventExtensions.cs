#nullable enable

using GraphQLEngine.Services.Events;

namespace GraphQLEngine.Services.Events;

/// <summary>
/// Provides extension methods for <see cref="Event"/> objects.
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Determines if two events have the same type and timestamp.
    /// </summary>
    /// <param name="event">The event to compare.</param>
    /// <param name="other">The other event to compare.</param>
    /// <returns>True if the events have the same type and timestamp, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> or <paramref name="other"/> is null.</exception>
    public static bool HasSameTypeAndTimestamp(this Event @event, Event other)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(other);

        return @event.GetType() == other.GetType() && @event.Timestamp.Equals(other.Timestamp);
    }

    /// <summary>
    /// Adds a metadata entry to the event.
    /// </summary>
    /// <param name="event">The event to add metadata to.</param>
    /// <param name="key">The key of the metadata entry.</param>
    /// <param name="value">The value of the metadata entry.</param>
    /// <returns>The event with the added metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    public static Event AddMetadataEntry(this Event @event, string key, object value)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrEmpty(key);

        @event.Metadata[key] = value;
        return @event;
    }

    /// <summary>
    /// Gets a metadata entry from the event.
    /// </summary>
    /// <param name="event">The event to get metadata from.</param>
    /// <param name="key">The key of the metadata entry.</param>
    /// <returns>The metadata entry value, or null if not found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or empty.</exception>
    public static object? GetMetadataEntry(this Event @event, string key)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrEmpty(key);

        if (@event.Metadata.TryGetValue(key, out object? value))
        {
            return value;
        }

        return null;
    }
}