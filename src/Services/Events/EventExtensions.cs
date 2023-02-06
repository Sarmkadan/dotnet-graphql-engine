#nullable enable

using GraphQLEngine.Services.Events;

namespace GraphQLEngine.Services.Events;

/// <summary>
/// Extensions for Event
/// </summary>
public static class EventExtensions
{
    /// <summary>
    /// Determines if two events have the same type and timestamp
    /// </summary>
    /// <param name="event">The event to compare</param>
    /// <param name="other">The other event to compare</param>
    /// <returns>True if the events have the same type and timestamp, false otherwise</returns>
    public static bool HasSameTypeAndTimestamp(this Event @event, Event other)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(other);

        return @event.GetType() == other.GetType() && @event.Timestamp == other.Timestamp;
    }

    /// <summary>
    /// Adds a metadata entry to the event
    /// </summary>
    /// <param name="event">The event to add metadata to</param>
    /// <param name="key">The key of the metadata entry</param>
    /// <param name="value">The value of the metadata entry</param>
    /// <returns>The event with the added metadata</returns>
    public static Event AddMetadataEntry(this Event @event, string key, object value)
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentException.ThrowIfNullOrEmpty(key);

        @event.Metadata[key] = value;
        return @event;
    }

    /// <summary>
    /// Gets a metadata entry from the event
    /// </summary>
    /// <param name="event">The event to get metadata from</param>
    /// <param name="key">The key of the metadata entry</param>
    /// <returns>The metadata entry value, or null if not found</returns>
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
