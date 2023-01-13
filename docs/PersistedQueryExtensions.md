# PersistedQueryExtensions

The `PersistedQueryExtensions` class provides configuration options for enabling and customizing persisted query functionality in a GraphQL server. Persisted queries allow clients to send query hashes instead of full query strings, improving performance and security by reducing payload size and enabling query allowlisting. This extension integrates with the `IServiceCollection` to configure the persisted query middleware.

## API

### `AddPersistedQueries(IServiceCollection services, Action<PersistedQueryOptions> configure)`
Configures persisted query services with customizable options.

**Parameters:**
- `services` (`IServiceCollection`): The service collection to which the persisted query services are added.
- `configure` (`Action<PersistedQueryOptions>`): A delegate that configures the `PersistedQueryOptions` for the persisted query middleware.

**Returns:**
- `IServiceCollection`: The service collection for method chaining.

**Remarks:**
- This method registers the persisted query middleware and its dependencies.
- If `configure` is `null`, default options are used.

---

### `AddPersistedQueries(IServiceCollection services)`
Configures persisted query services with default options.

**Parameters:**
- `services` (`IServiceCollection`): The service collection to which the persisted query services are added.

**Returns:**
- `IServiceCollection`: The service collection for method chaining.

**Remarks:**
- Equivalent to calling `AddPersistedQueries(services, null)`.

---

### `EnforceHashVerification`
Gets or sets a value indicating whether the middleware should verify that incoming query hashes match the computed hash of the query string.

**Type:** `bool`
**Default:** `true`

**Remarks:**
- If `true`, the middleware will reject requests where the provided hash does not match the computed hash of the query.
- If `false`, the middleware will skip hash verification, which may be useful for debugging or development environments.

---

### `MaxIndexSize`
Gets or sets the maximum size (in bytes) of the persisted query index cache.

**Type:** `int`
**Default:** `10_000_000` (10 MB)

**Remarks:**
- The index cache stores query hashes and their corresponding query strings.
- If the cache exceeds this size, the oldest entries are evicted to make room for new ones.
- Setting this to `0` disables the cache size limit (not recommended for production).

---

### `AllowlistOnly`
Gets or sets a value indicating whether the middleware should only allow queries that are explicitly allowlisted.

**Type:** `bool`
**Default:** `false`

**Remarks:**
- If `true`, only queries present in the persisted query index will be executed. Unknown queries will be rejected.
- If `false`, unknown queries may be executed if hash verification passes (or is disabled).

---

### `ReturnNotFoundError`
Gets or sets a value indicating whether the middleware should return a `404 Not Found` error for unknown queries instead of a generic error.

**Type:** `bool`
**Default:** `false`

**Remarks:**
- If `true`, unknown queries will result in a `404 Not Found` response.
- If `false`, a generic error (e.g., "Persisted query not found") is returned.

---

### `Validate`
Gets or sets a value indicating whether the middleware should validate the query syntax before execution.

**Type:** `bool`
**Default:** `true`

**Remarks:**
- If `true`, the middleware will parse the query to ensure it is syntactically valid before execution.
- If `false`, query validation is skipped, which may improve performance but risks runtime errors for invalid queries.

## Usage

### Example 1: Basic Configuration
