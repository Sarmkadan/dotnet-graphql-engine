#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Integration;

/// <summary>
/// Integration layer for external APIs
/// Provides common patterns for API communication with caching and error handling
/// </summary>
sealed public class ExternalApiIntegration : IDisposable
{
    private readonly HttpClientFactory _httpClientFactory;
    private readonly ILogger<ExternalApiIntegration> _logger;
    private readonly Dictionary<string, ExternalApiEndpoint> _endpoints;

    public ExternalApiIntegration(
        HttpClientFactory httpClientFactory,
        ILogger<ExternalApiIntegration> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _endpoints = new Dictionary<string, ExternalApiEndpoint>();
    }

    /// <summary>
    /// Registers an external API endpoint
    /// </summary>
    public void RegisterEndpoint(string name, string baseUrl, ExternalApiEndpointConfig config)
    {
        var endpoint = new ExternalApiEndpoint
        {
            Name = name,
            BaseUrl = baseUrl,
            Config = config,
            CreatedAt = DateTime.UtcNow
        };

        _endpoints[name] = endpoint;
        _logger.LogInformation("External API endpoint registered: {EndpointName} -> {BaseUrl}", name, baseUrl);
    }

    /// <summary>
    /// Calls a GET endpoint on an external API
    /// </summary>
    public async Task<ApiResponse<T>> GetAsync<T>(string endpointName, string path, Dictionary<string, string>? queryParams = null) where T : class
    {
        if (!_endpoints.TryGetValue(endpointName, out var endpoint))
        {
            return ApiResponse<T>.Failure($"Endpoint '{endpointName}' not found");
        }

        try
        {
            var url = BuildUrl(endpoint.BaseUrl, path, queryParams);
            _logger.LogDebug("GET request to {EndpointName}: {Url}", endpointName, url);

            var response = await _httpClientFactory.GetAsync(url, endpointName);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = System.Text.Json.JsonSerializer.Deserialize<T>(content);
                endpoint.LastSuccessfulCall = DateTime.UtcNow;
                endpoint.CallCount++;

                return ApiResponse<T>.Ok(data);
            }

            endpoint.FailureCount++;
            _logger.LogWarning("GET request failed: {StatusCode}", response.StatusCode);

            return ApiResponse<T>.Failure($"HTTP {response.StatusCode}");
        }
        catch (Exception ex)
        {
            endpoint.FailureCount++;
            _logger.LogError(ex, "Error calling external API: {EndpointName}", endpointName);

            return ApiResponse<T>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Calls a POST endpoint on an external API
    /// </summary>
    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string endpointName,
        string path,
        TRequest data) where TRequest : class where TResponse : class
    {
        if (!_endpoints.TryGetValue(endpointName, out var endpoint))
        {
            return ApiResponse<TResponse>.Failure($"Endpoint '{endpointName}' not found");
        }

        try
        {
            var url = BuildUrl(endpoint.BaseUrl, path);
            _logger.LogDebug("POST request to {EndpointName}: {Url}", endpointName, url);

            var response = await _httpClientFactory.PostJsonAsync(url, data, endpointName);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<TResponse>(content);
                endpoint.LastSuccessfulCall = DateTime.UtcNow;
                endpoint.CallCount++;

                return ApiResponse<TResponse>.Ok(result);
            }

            endpoint.FailureCount++;
            _logger.LogWarning("POST request failed: {StatusCode}", response.StatusCode);

            return ApiResponse<TResponse>.Failure($"HTTP {response.StatusCode}");
        }
        catch (Exception ex)
        {
            endpoint.FailureCount++;
            _logger.LogError(ex, "Error calling external API: {EndpointName}", endpointName);

            return ApiResponse<TResponse>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Checks the health of an external API endpoint
    /// </summary>
    public async Task<bool> HealthCheckAsync(string endpointName)
    {
        if (!_endpoints.TryGetValue(endpointName, out var endpoint))
        {
            _logger.LogWarning("Endpoint '{EndpointName}' not found for health check", endpointName);
            return false;
        }

        try
        {
            var url = $"{endpoint.BaseUrl}/health";
            var response = await _httpClientFactory.GetAsync(url, endpointName);

            var isHealthy = response.IsSuccessStatusCode;
            if (isHealthy)
            {
                endpoint.IsHealthy = true;
                endpoint.LastHealthCheckAt = DateTime.UtcNow;
            }
            else
            {
                endpoint.IsHealthy = false;
            }

            return isHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed for {EndpointName}", endpointName);
            endpoint.IsHealthy = false;
            return false;
        }
    }

    /// <summary>
    /// Gets statistics for all registered endpoints
    /// </summary>
    public ExternalApiStatistics GetStatistics()
    {
        var stats = new ExternalApiStatistics
        {
            TotalEndpoints = _endpoints.Count,
            HealthyEndpoints = _endpoints.Values.Count(e => e.IsHealthy),
            TotalCalls = _endpoints.Values.Sum(e => e.CallCount),
            TotalFailures = _endpoints.Values.Sum(e => e.FailureCount),
            Endpoints = _endpoints.Values.Select(e => new EndpointStats
            {
                Name = e.Name,
                IsHealthy = e.IsHealthy,
                CallCount = e.CallCount,
                FailureCount = e.FailureCount,
                LastSuccessfulCall = e.LastSuccessfulCall,
                LastHealthCheckAt = e.LastHealthCheckAt
            }).ToList()
        };

        stats.SuccessRate = stats.TotalCalls > 0
            ? (double)(stats.TotalCalls - stats.TotalFailures) / stats.TotalCalls
            : 0;

        return stats;
    }

    /// <summary>
    /// Builds a URL from base URL and path
    /// </summary>
    private string BuildUrl(string baseUrl, string path, Dictionary<string, string>? queryParams = null)
    {
        var url = $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

        if (queryParams is not null && queryParams.Count > 0)
        {
            var queryString = string.Join("&", queryParams.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            url = $"{url}?{queryString}";
        }

        return url;
    }

    public void Dispose()
    {
        _endpoints.Clear();
    }
}

/// <summary>
/// External API endpoint registration
/// </summary>
sealed public class ExternalApiEndpoint
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public ExternalApiEndpointConfig Config { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSuccessfulCall { get; set; }
    public DateTime? LastHealthCheckAt { get; set; }
    public long CallCount { get; set; }
    public long FailureCount { get; set; }
    public bool IsHealthy { get; set; } = true;
}

/// <summary>
/// External API endpoint configuration
/// </summary>
sealed public class ExternalApiEndpointConfig
{
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
    public bool RequiresAuthentication { get; set; }
    public string? AuthenticationToken { get; set; }
    public Dictionary<string, string>? DefaultHeaders { get; set; }
}

/// <summary>
/// API response wrapper
/// </summary>
sealed public class ApiResponse<T> where T : class
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T? data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponse<T> Failure(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error
        };
    }
}

/// <summary>
/// External API statistics
/// </summary>
sealed public class ExternalApiStatistics
{
    public int TotalEndpoints { get; set; }
    public int HealthyEndpoints { get; set; }
    public long TotalCalls { get; set; }
    public long TotalFailures { get; set; }
    public double SuccessRate { get; set; }
    public List<EndpointStats> Endpoints { get; set; } = new();
}

/// <summary>
/// Individual endpoint statistics
/// </summary>
sealed public class EndpointStats
{
    public string Name { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public long CallCount { get; set; }
    public long FailureCount { get; set; }
    public DateTime? LastSuccessfulCall { get; set; }
    public DateTime? LastHealthCheckAt { get; set; }
}
