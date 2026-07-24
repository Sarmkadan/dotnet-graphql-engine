using System;
using System.Collections.Generic;
using Xunit;
using GraphQLEngine.Services.Events;

namespace GraphQLEngine.Tests;

public class EventExtensionsTests
{
    // A simple concrete Event implementation for testing purposes.
    private sealed class TestEvent : Event
    {
        public TestEvent(DateTime timestamp)
        {
            Timestamp = timestamp;
            Metadata = new Dictionary<string, object>();
        }
    }

    [Fact]
    public void HasSameTypeAndTimestamp_SameTypeAndTimestamp_ReturnsTrue()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var ev1 = new TestEvent(timestamp);
        var ev2 = new TestEvent(timestamp);

        // Act
        var result = ev1.HasSameTypeAndTimestamp(ev2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasSameTypeAndTimestamp_DifferentTimestamp_ReturnsFalse()
    {
        // Arrange
        var ev1 = new TestEvent(DateTime.UtcNow);
        var ev2 = new TestEvent(DateTime.UtcNow.AddSeconds(1));

        // Act
        var result = ev1.HasSameTypeAndTimestamp(ev2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasSameTypeAndTimestamp_DifferentType_ReturnsFalse()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var ev1 = new TestEvent(timestamp);
        var ev2 = new DerivedTestEvent(timestamp); // Different derived type

        // Act
        var result = ev1.HasSameTypeAndTimestamp(ev2);

        // Assert
        Assert.False(result);
    }

    private sealed class DerivedTestEvent : Event
    {
        public DerivedTestEvent(DateTime timestamp)
        {
            Timestamp = timestamp;
            Metadata = new Dictionary<string, object>();
        }
    }

    [Fact]
    public void HasSameTypeAndTimestamp_NullEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var ev = new TestEvent(DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ev.HasSameTypeAndTimestamp(null!));
        Assert.Throws<ArgumentNullException>(() => ((Event)null!).HasSameTypeAndTimestamp(ev));
    }

    [Fact]
    public void AddMetadataEntry_ValidKey_AddsMetadataAndReturnsSameInstance()
    {
        // Arrange
        var ev = new TestEvent(DateTime.UtcNow);
        const string key = "key";
        var value = 123;

        // Act
        var returned = ev.AddMetadataEntry(key, value);

        // Assert
        Assert.Same(ev, returned);
        Assert.True(ev.Metadata.ContainsKey(key));
        Assert.Equal(value, ev.Metadata[key]);
    }

    [Fact]
    public void AddMetadataEntry_NullEvent_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((Event)null!).AddMetadataEntry("k", "v"));
    }

    [Fact]
    public void AddMetadataEntry_NullOrEmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var ev = new TestEvent(DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ev.AddMetadataEntry(null!, "value"));
        Assert.Throws<ArgumentException>(() => ev.AddMetadataEntry(string.Empty, "value"));
    }

    [Fact]
    public void GetMetadataEntry_ExistingKey_ReturnsValue()
    {
        // Arrange
        var ev = new TestEvent(DateTime.UtcNow);
        ev.Metadata["existing"] = "value";

        // Act
        var result = ev.GetMetadataEntry("existing");

        // Assert
        Assert.Equal("value", result);
    }

    [Fact]
    public void GetMetadataEntry_NonExistingKey_ReturnsNull()
    {
        // Arrange
        var ev = new TestEvent(DateTime.UtcNow);

        // Act
        var result = ev.GetMetadataEntry("missing");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetMetadataEntry_NullEvent_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ((Event)null!).GetMetadataEntry("key"));
    }

    [Fact]
    public void GetMetadataEntry_NullOrEmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var ev = new TestEvent(DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ev.GetMetadataEntry(null!));
        Assert.Throws<ArgumentException>(() => ev.GetMetadataEntry(string.Empty));
    }
}
