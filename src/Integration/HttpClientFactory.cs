// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Integration;

/// <summary>
/// Factory for creating and managing HTTP client instances
/// Provides singleton pattern and connection pooling
/// </summary>
public class HttpClientFactory : IDisposable
{
    private readonly ILogger<HttpClientFactory> _logger;
    private readonly Dictionary<string, HttpClient> _clients;
    private readonly HttpClientFactoryOptions _options;
    private readonly object _lockObject = new();

    public HttpClientFactory(
        ILogger<HttpClientFactory> logger,
        HttpClientFactoryOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? HttpClientFactoryOptions.Default();
        _clients = new Dictionary<string, HttpClient>();
    }

    /// <summary>
    /// Gets or creates an HTTP client
    /// </summary>
    public HttpClient GetClient(string name = "default")
    {
        lock (_lockObject)
        {
            if (_clients.TryGetValue(name, out var client))
                return client;

            var newClient = CreateHttpClient(name);
            _clients[name] = newClient;
            _logger.LogDebug("Created HTTP client: {ClientName}", name);
            return newClient;
        }
    }

    /// <summary>
    /// Creates an HTTP client with custom configuration
    /// </summary>
    public HttpClient CreateNamedClient(string name, Action<HttpClient> configure)
    {
        lock (_lockObject)
        {
            if (_clients.ContainsKey(name))
                throw new InvalidOperationException($"Client '{name}' already exists");

            var client = CreateHttpClient(name);
            configure(client);

            _clients[name] = client;
            _logger.LogDebug("Created named HTTP client: {ClientName}", name);
            return client;
        }
    }

    /// <summary>
    /// Makes a GET request
    /// </summary>
    public async Task<HttpResponseMessage> GetAsync(string url, string clientName = "default")
    {
        var client = GetClient(clientName);
        _logger.LogDebug("GET request: {Url}", url);

        try
        {
            return await client.GetAsync(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GET request failed: {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Makes a POST request
    /// </summary>
    public async Task<HttpResponseMessage> PostAsync(
        string url,
        HttpContent content,
        string clientName = "default")
    {
        var client = GetClient(clientName);
        _logger.LogDebug("POST request: {Url}", url);

        try
        {
            return await client.PostAsync(url, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST request failed: {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Makes a POST request with JSON content
    /// </summary>
    public async Task<HttpResponseMessage> PostJsonAsync<T>(
        string url,
        T data,
        string clientName = "default")
    {
        var json = System.Text.Json.JsonSerializer.Serialize(data);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        return await PostAsync(url, content, clientName);
    }

    /// <summary>
    /// Makes a PUT request
    /// </summary>
    public async Task<HttpResponseMessage> PutAsync(
        string url,
        HttpContent content,
        string clientName = "default")
    {
        var client = GetClient(clientName);
        _logger.LogDebug("PUT request: {Url}", url);

        try
        {
            return await client.PutAsync(url, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PUT request failed: {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Makes a DELETE request
    /// </summary>
    public async Task<HttpResponseMessage> DeleteAsync(string url, string clientName = "default")
    {
        var client = GetClient(clientName);
        _logger.LogDebug("DELETE request: {Url}", url);

        try
        {
            return await client.DeleteAsync(url);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DELETE request failed: {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Creates a new HTTP client with configured settings
    /// </summary>
    private HttpClient CreateHttpClient(string name)
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = _options.AllowAutoRedirect,
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
            MaxConnectionsPerServer = _options.MaxConnectionsPerServer
        };

        var client = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = _options.RequestTimeout
        };

        // Add default headers
        if (_options.DefaultHeaders != null)
        {
            foreach (var header in _options.DefaultHeaders)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        client.DefaultRequestHeaders.Add("User-Agent", "dotnet-graphql-engine/1.0.0");

        return client;
    }

    /// <summary>
    /// Removes a named client
    /// </summary>
    public void RemoveClient(string name)
    {
        lock (_lockObject)
        {
            if (_clients.TryGetValue(name, out var client))
            {
                client.Dispose();
                _clients.Remove(name);
                _logger.LogDebug("Removed HTTP client: {ClientName}", name);
            }
        }
    }

    /// <summary>
    /// Gets client statistics
    /// </summary>
    public HttpClientFactoryStatistics GetStatistics()
    {
        lock (_lockObject)
        {
            return new HttpClientFactoryStatistics
            {
                ClientCount = _clients.Count,
                ClientNames = _clients.Keys.ToList()
            };
        }
    }

    public void Dispose()
    {
        lock (_lockObject)
        {
            foreach (var client in _clients.Values)
                client.Dispose();

            _clients.Clear();
        }
    }
}

/// <summary>
/// HTTP client factory options
/// </summary>
public class HttpClientFactoryOptions
{
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool AllowAutoRedirect { get; set; } = true;
    public int MaxConnectionsPerServer { get; set; } = 10;
    public Dictionary<string, string>? DefaultHeaders { get; set; }

    public static HttpClientFactoryOptions Default()
    {
        return new HttpClientFactoryOptions();
    }
}

/// <summary>
/// HTTP client factory statistics
/// </summary>
public class HttpClientFactoryStatistics
{
    public int ClientCount { get; set; }
    public List<string> ClientNames { get; set; } = new();
}
