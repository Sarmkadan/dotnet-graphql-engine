#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.ValueObjects;

/// <summary>
/// Configuration for GraphQL schema stitching
/// </summary>
sealed public class SchemaStitchingConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public int TimeoutMs { get; set; } = 5000;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;

    private readonly List<RemoteSchema> _remoteSchemas = new();
    public IReadOnlyList<RemoteSchema> RemoteSchemas => _remoteSchemas.AsReadOnly();

    private readonly Dictionary<string, string> _typeMapping = new();
    public IReadOnlyDictionary<string, string> TypeMapping => _typeMapping.AsReadOnly();

    public SchemaStitchingConfig()
    {
    }

    public SchemaStitchingConfig(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Adds a remote schema to be stitched
    /// </summary>
    public void AddRemoteSchema(RemoteSchema schema)
    {
        if (schema is null) throw new ArgumentNullException(nameof(schema));

        if (_remoteSchemas.Any(s => s.Name == schema.Name))
            throw new InvalidOperationException($"Remote schema '{schema.Name}' already exists");

        _remoteSchemas.Add(schema);
    }

    /// <summary>
    /// Removes a remote schema by name
    /// </summary>
    public bool RemoveRemoteSchema(string schemaName)
    {
        var schema = _remoteSchemas.FirstOrDefault(s => s.Name == schemaName);
        if (schema is null) return false;

        _remoteSchemas.Remove(schema);
        return true;
    }

    /// <summary>
    /// Maps a local type name to a remote type name
    /// </summary>
    public void MapType(string localTypeName, string remoteTypeName)
    {
        if (string.IsNullOrEmpty(localTypeName))
            throw new ArgumentException("Local type name cannot be empty", nameof(localTypeName));

        if (string.IsNullOrEmpty(remoteTypeName))
            throw new ArgumentException("Remote type name cannot be empty", nameof(remoteTypeName));

        _typeMapping[localTypeName] = remoteTypeName;
    }

    /// <summary>
    /// Gets the remote type name for a local type
    /// </summary>
    public string? GetRemoteTypeName(string localTypeName)
    {
        _typeMapping.TryGetValue(localTypeName, out var remoteName);
        return remoteName;
    }

    /// <summary>
    /// Validates the stitching configuration
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Stitching configuration name is required");

        if (_remoteSchemas.Count == 0)
            errors.Add("At least one remote schema must be configured");

        if (TimeoutMs < 100)
            errors.Add("Timeout must be at least 100ms");

        if (MaxRetries < 0)
            errors.Add("Max retries cannot be negative");

        foreach (var schema in _remoteSchemas)
        {
            if (!schema.Validate(out var schemaErrors))
                errors.AddRange(schemaErrors.Select(e => $"Remote schema '{schema.Name}': {e}"));
        }

        return errors.Count == 0;
    }
}

/// <summary>
/// Represents a remote GraphQL schema to be stitched
/// </summary>
sealed public class RemoteSchema
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public bool RequiresAuthentication { get; set; } = false;
    public string? AuthHeader { get; set; }

    public RemoteSchema()
    {
    }

    public RemoteSchema(string name, string endpoint)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
    }

    /// <summary>
    /// Sets authentication header for this remote schema
    /// </summary>
    public void SetAuthenticationHeader(string header)
    {
        if (string.IsNullOrEmpty(header))
            throw new ArgumentException("Auth header cannot be empty", nameof(header));

        RequiresAuthentication = true;
        AuthHeader = header;
    }

    /// <summary>
    /// Validates the remote schema configuration
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Remote schema name is required");

        if (string.IsNullOrWhiteSpace(Endpoint))
            errors.Add("Remote schema endpoint is required");

        if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out _))
            errors.Add("Remote schema endpoint must be a valid URL");

        if (RequiresAuthentication && string.IsNullOrEmpty(AuthHeader))
            errors.Add("Authentication header is required when authentication is enabled");

        return errors.Count == 0;
    }

    /// <summary>
    /// Gets the endpoint as a Uri
    /// </summary>
    public Uri GetEndpointUri()
    {
        return new Uri(Endpoint);
    }
}
