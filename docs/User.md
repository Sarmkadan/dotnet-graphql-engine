# User

The `User` record models a user entity within the GraphQL schema of the `dotnet-graphql-engine` project. It provides a static entry point to launch the engine and defines nested record types for related entities such as posts and comments.

## API

### `public static async Task Run()`

**Purpose:** Starts the GraphQL server hosted by the engine. This method initializes required services, configures the endpoint, and begins listening for incoming requests.

**Parameters:** None.

**Return Value:** A `Task` that completes when the server shuts down. The returned task can be awaited to block the calling thread until the server stops.

**Exceptions:**  
- `InvalidOperationException` if the engine has not been properly configured (e.g., missing schema or services).  
- `System.Net.Sockets.SocketException` if the configured port is already in use or unavailable.  
- Any exception thrown during dependency injection or middleware initialization propagates through the returned task.

### `public record Post`

**Purpose:** Represents a blog post associated with a user. As a record, it provides immutable data storage with compiler‑generated public init‑only properties corresponding to the post’s fields (e.g., identifier, title, content, author). It is intended for use as a GraphQL object type.

**Parameters:** The record’s primary constructor parameters match its properties; they are supplied via object initialization syntax or positional arguments when creating an instance.

**Return Value:** Not applicable; the type itself is returned when instantiated.

**Exceptions:** The record type does not throw exceptions during normal construction or property access. Exceptions may arise only from user‑provided property values (e.g., validation logic outside the record).

### `public record Comment`

**Purpose:** Models a comment made on a post. Like `Post`, it is an immutable record with compiler‑generated properties that reflect the comment’s fields (e.g., identifier, text, author, associated post). It serves as a GraphQL object type.

**Parameters:** Constructor parameters correspond to the record’s properties and are supplied at instantiation.

**Return Value:** Not applicable; instances are created directly.

**Exceptions:** No exceptions are thrown by the record’s generated members; any validation must be performed externally.

## Usage

```csharp
// Starting the GraphQL engine.
await User.Run();
```

```csharp
// Creating instances of the related types
    = new User(csharp)
// Defining a user with related post with
var user = new User { Id = 1, Username: "alice"
var user = new User { Id = 1, Username = "alice" };

// Creating a post authored by the user.
var post = new Post
{
    Id = 10,
    Title = "First post",
    Content = "Hello world!",
    AuthorId = user.Id
};

// Adding a comment to the post.
var comment = new Comment
{
    Id = 100,
    Text = "Nice post!",
    AuthorId = user.Id,
    PostId = post.Id
};

// The instances can now be passed to GraphQL resolvers or used in tests.
```

## Notes

- The `Run` method should be invoked only once per application lifetime. Subsequent calls may attempt to bind to the same endpoint and result in a `SocketException`.
- While the `Run` method is thread‑safe with respect to concurrent invocation, invoking it from multiple threads simultaneously is not recommended and may lead to undefined behavior.
- The `User`, `Post`, and `Comment` records are immutable; their properties cannot be modified after creation, making them safe to share across threads without additional synchronization.
- Because the records rely on compiler‑generated constructors, all properties must be supplied at initialization; omitting a required property results in a compile‑time error.
- No built‑in validation is performed on property values; consumers should enforce any domain‑specific constraints (e.g., non‑null strings, identifier ranges) before persisting or transmitting the instances.
