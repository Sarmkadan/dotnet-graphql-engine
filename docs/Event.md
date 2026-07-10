# Event

The `Event` type represents a discrete occurrence or message within the `dotnet-graphql-engine` system, used for inter-component communication via an event bus pattern. It encapsulates event metadata, payload data, and timing information, enabling decoupled interaction between publishers and subscribers. The type is designed for both synchronous and asynchronous event handling, with support for tracking subscription counts, logging, and statistics.

## API

### `EventBus`

The central dispatcher for events, managing subscriptions and message routing. All event lifecycle operations flow through this instance.

### `void Subscribe<TEvent>(Action<TEvent> handler)`

Registers a synchronous handler for events of type `TEvent`. The handler is invoked immediately when an event of the specified type is published.

- **Parameters**:
  - `handler` (Action<TEvent>): The synchronous callback to execute when an event is published.
- **Throws**:
  - `ArgumentNullException`: If `handler` is `null`.
- **Remarks**: Multiple handlers can be registered for the same event type. Handlers are invoked in the order they were subscribed.

### `void SubscribeAsync<TEvent>(Func<TEvent, Task> handler)`

Registers an asynchronous handler for events of type `TEvent`. The handler is awaited when an event is published.

- **Parameters**:
  - `handler` (Func<TEvent, Task>): The asynchronous callback to execute when an event is published.
- **Throws**:
  - `ArgumentNullException`: If `handler` is `null`.
- **Remarks**: Asynchronous handlers are executed sequentially per event type, but may run concurrently across different event types. Exceptions from handlers are not propagated to the publisher.

### `void Publish<TEvent>(TEvent @event)`

Publishes an event of type `TEvent` synchronously. All registered synchronous handlers are invoked immediately in registration order.

- **Parameters**:
  - `@event` (TEvent): The event payload to publish.
- **Throws**:
  - `ArgumentNullException`: If `@event` is `null`.
- **Remarks**: Synchronous publishing blocks until all synchronous handlers complete. Asynchronous handlers are not invoked during synchronous publish.

### `async Task PublishAsync<TEvent>(TEvent @event)`

Publishes an event of type `TEvent` asynchronously. All registered synchronous and asynchronous handlers are invoked, with asynchronous handlers awaited.

- **Parameters**:
  - `@event` (TEvent): The event payload to publish.
- **Returns**:
  - `Task`: A task representing the asynchronous publish operation.
- **Throws**:
  - `ArgumentNullException`: If `@event` is `null`.
- **Remarks**: Asynchronous handlers are executed in registration order per event type. Exceptions from handlers are captured and logged but do not interrupt other handlers.

### `void Unsubscribe<TEvent>(Action<TEvent> handler)`

Removes a previously registered synchronous handler for events of type `TEvent`.

- **Parameters**:
  - `handler` (Action<TEvent>): The handler to remove.
- **Remarks**: If the handler was not previously registered, this method has no effect. Equality is reference-based.

### `int GetSubscriberCount<TEvent>()`

Returns the number of handlers currently subscribed to events of type `TEvent`.

- **Returns**:
  - `int`: The count of registered handlers for `TEvent`.
- **Type Parameters**:
  - `TEvent`: The event type to query.

### `List<EventLog> GetEventLog()`

Retrieves a copy of the event log, which records all published events and their metadata.

- **Returns**:
  - `List<EventLog>`: A list of event logs, each representing a published event.
- **Remarks**: The returned list is a snapshot and does not reflect subsequent changes. Modifications to the list do not affect the internal log.

### `void ClearEventLog()`

Removes all entries from the event log.

- **Remarks**: This operation does not affect active subscriptions or future event logging.

### `EventBusStatistics GetStatistics()`

Returns aggregated statistics about the event bus, including total subscriptions and event counts.

- **Returns**:
  - `EventBusStatistics`: An object containing subscription and event metrics.
- **Remarks**: The statistics reflect the state at the time of the call and may be immediately outdated by concurrent operations.

### `void Dispose()`

Releases all resources used by the `EventBus`, including unregistering all handlers and clearing the event log.

- **Remarks**: After disposal, the `EventBus` instance should not be used. Attempting to publish or subscribe will result in undefined behavior.

### `public string Id`

A unique identifier for the event instance, generated at creation time.

### `public DateTime Timestamp`

The date and time when the event was created.

### `public string? Source`

An optional identifier for the component or module that originated the event.

### `public Dictionary<string, object> Metadata`

A collection of key-value pairs providing additional context about the event. Keys are case-sensitive.

### `public string Id`

A unique identifier for the event log entry.

### `public string EventType`

The fully qualified type name of the event payload.

### `public DateTime Timestamp`

The date and time when the event was published.

### `public object? Data`

The serialized event payload, or `null` if the event carries no data.

### `public int TotalSubscriptions`

The total number of active subscriptions across all event types at the time of query.

## Usage

### Example 1: Basic Synchronous Event Handling
