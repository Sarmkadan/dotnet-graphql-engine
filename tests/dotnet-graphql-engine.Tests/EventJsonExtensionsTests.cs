using System;
using Xunit;
using GraphQLEngine.Services.Events;

namespace GraphQLEngine.Tests;

public class EventJsonExtensionsTests
{
    private sealed class TestEvent : Event
    {
        public TestEvent(DateTime timestamp)
        {
            Timestamp = timestamp;
        }
    }

    [Fact]
    public void ToJson_ValidEvent_ProducesDeserializableJson()
    {
        var timestamp = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var ev = new TestEvent(timestamp);

        var json = ev.ToJson();

        Assert.False(string.IsNullOrWhiteSpace(json));
        Assert.Contains("\"timestamp\"", json);
        Assert.Contains(timestamp.ToString("yyyy-MM-dd"), json);
    }

    [Fact]
    public void ToJson_Indented_ProducesMultilineOutput()
    {
        var ev = new TestEvent(DateTime.UtcNow);

        var compact = ev.ToJson(indented: false);
        var indented = ev.ToJson(indented: true);

        Assert.DoesNotContain("\n", compact);
        Assert.Contains("\n", indented);
    }

    [Fact]
    public void ToJson_NullEvent_ThrowsArgumentNullException()
    {
        Event? ev = null;

        Assert.Throws<ArgumentNullException>(() => ev!.ToJson());
    }

    [Fact]
    public void FromJson_MalformedJson_ReturnsNull()
    {
        // Malformed JSON fails during parsing before the abstract-type check is
        // reached, so the JsonException is caught and null is returned.
        var result = EventJsonExtensions.FromJson("not valid json at all");

        Assert.Null(result);
    }

    [Fact]
    public void FromJson_AbstractEventType_ThrowsNotSupportedException()
    {
        // Event is an abstract base class with no polymorphic discriminator
        // configured, so System.Text.Json cannot materialize it directly and
        // raises NotSupportedException, which FromJson does not swallow.
        var ev = new TestEvent(DateTime.UtcNow);
        var json = ev.ToJson();

        Assert.Throws<NotSupportedException>(() => EventJsonExtensions.FromJson(json));
    }

    [Fact]
    public void FromJson_EmptyInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => EventJsonExtensions.FromJson(string.Empty));
    }

    [Fact]
    public void FromJson_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => EventJsonExtensions.FromJson(null!));
    }

    [Fact]
    public void TryFromJson_MalformedJson_ReturnsFalseAndNull()
    {
        var success = EventJsonExtensions.TryFromJson("not valid json at all", out var value);

        Assert.False(success);
        Assert.Null(value);
    }

    [Fact]
    public void TryFromJson_AbstractEventType_ThrowsNotSupportedException()
    {
        var ev = new TestEvent(DateTime.UtcNow);
        var json = ev.ToJson();

        Assert.Throws<NotSupportedException>(() => EventJsonExtensions.TryFromJson(json, out _));
    }

    [Fact]
    public void TryFromJson_NullOrEmptyInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => EventJsonExtensions.TryFromJson(string.Empty, out _));
        Assert.Throws<ArgumentNullException>(() => EventJsonExtensions.TryFromJson(null!, out _));
    }
}
