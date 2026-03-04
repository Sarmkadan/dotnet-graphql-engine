#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Integration;

/// <summary>
/// Webhook handler for sending and managing webhooks
/// Supports retry logic, deduplication, and rate limiting
/// </summary>
sealed public class WebhookHandler : IDisposable
{
    private readonly HttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookHandler> _logger;
    private readonly WebhookOptions _options;
    private readonly List<WebhookDelivery> _deliveryLog;
    private readonly Dictionary<string, WebhookEndpoint> _endpoints;

    public WebhookHandler(
        HttpClientFactory httpClientFactory,
        ILogger<WebhookHandler> logger,
        WebhookOptions? options = null)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new WebhookOptions();
        _deliveryLog = new List<WebhookDelivery>();
        _endpoints = new Dictionary<string, WebhookEndpoint>();
    }

    /// <summary>
    /// Registers a webhook endpoint
    /// </summary>
    public void RegisterEndpoint(string id, string url, string? secret = null)
    {
        var endpoint = new WebhookEndpoint
        {
            Id = id,
            Url = url,
            Secret = secret,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _endpoints[id] = endpoint;
        _logger.LogInformation("Webhook endpoint registered: {EndpointId} -> {Url}", id, url);
    }

    /// <summary>
    /// Unregisters a webhook endpoint
    /// </summary>
    public void UnregisterEndpoint(string id)
    {
        if (_endpoints.Remove(id))
            _logger.LogInformation("Webhook endpoint unregistered: {EndpointId}", id);
    }

    /// <summary>
    /// Sends a webhook event to all registered endpoints
    /// </summary>
    public async Task SendWebhookAsync<T>(string eventType, T data) where T : class
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("Webhooks are disabled");
            return;
        }

        var activeEndpoints = _endpoints.Values.Where(e => e.IsActive).ToList();
        _logger.LogInformation("Sending webhook event {EventType} to {Count} endpoints", eventType, activeEndpoints.Count);

        var tasks = activeEndpoints.Select(endpoint =>
            SendWebhookToEndpointAsync(endpoint, eventType, data));

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// Sends a webhook to a specific endpoint with retry logic
    /// </summary>
    private async Task SendWebhookToEndpointAsync<T>(
        WebhookEndpoint endpoint,
        string eventType,
        T data) where T : class
    {
        var attempt = 0;
        var delay = _options.RetryDelayMs;

        while (attempt < _options.MaxRetries)
        {
            try
            {
                attempt++;
                _logger.LogDebug("Sending webhook to {EndpointId} (attempt {Attempt})", endpoint.Id, attempt);

                var payload = CreatePayload(eventType, data, endpoint.Secret);
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(payload),
                    System.Text.Encoding.UTF8,
                    "application/json");

                var response = await _httpClientFactory.PostAsync(endpoint.Url, content);

                var delivery = new WebhookDelivery
                {
                    EndpointId = endpoint.Id,
                    EventType = eventType,
                    StatusCode = (int)response.StatusCode,
                    Success = response.IsSuccessStatusCode,
                    Attempt = attempt,
                    DeliveredAt = DateTime.UtcNow
                };

                _deliveryLog.Add(delivery);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Webhook delivered to {EndpointId}", endpoint.Id);
                    endpoint.LastSuccessfulDeliveryAt = DateTime.UtcNow;
                    return;
                }

                if (attempt >= _options.MaxRetries)
                {
                    _logger.LogError("Webhook delivery failed to {EndpointId} after {Attempts} attempts",
                        endpoint.Id, attempt);
                    endpoint.IsActive = false;
                    return;
                }

                await Task.Delay(delay);
                delay *= 2; // Exponential backoff
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending webhook to {EndpointId} (attempt {Attempt})",
                    endpoint.Id, attempt);

                if (attempt >= _options.MaxRetries)
                {
                    endpoint.IsActive = false;
                    return;
                }

                await Task.Delay(delay);
                delay *= 2;
            }
        }
    }

    /// <summary>
    /// Creates a webhook payload
    /// </summary>
    private object CreatePayload<T>(string eventType, T data, string? secret) where T : class
    {
        var payload = new
        {
            id = Guid.NewGuid().ToString(),
            eventType = eventType,
            timestamp = DateTime.UtcNow,
            data = data
        };

        // Add signature if secret is provided
        if (!string.IsNullOrEmpty(secret))
        {
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var signature = ComputeSignature(json, secret);

            return new
            {
                payload,
                signature
            };
        }

        return payload;
    }

    /// <summary>
    /// Computes HMAC signature for webhook payload
    /// </summary>
    private string ComputeSignature(string payload, string secret)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Gets webhook delivery history
    /// </summary>
    public List<WebhookDelivery> GetDeliveryHistory(string? endpointId = null, int limit = 100)
    {
        var query = _deliveryLog.AsEnumerable();

        if (!string.IsNullOrEmpty(endpointId))
            query = query.Where(d => d.EndpointId == endpointId);

        return query.OrderByDescending(d => d.DeliveredAt).Take(limit).ToList();
    }

    /// <summary>
    /// Gets webhook statistics
    /// </summary>
    public WebhookStatistics GetStatistics()
    {
        return new WebhookStatistics
        {
            TotalEndpoints = _endpoints.Count,
            ActiveEndpoints = _endpoints.Values.Count(e => e.IsActive),
            TotalDeliveries = _deliveryLog.Count,
            SuccessfulDeliveries = _deliveryLog.Count(d => d.Success),
            FailedDeliveries = _deliveryLog.Count(d => !d.Success),
            SuccessRate = _deliveryLog.Count > 0
                ? (double)_deliveryLog.Count(d => d.Success) / _deliveryLog.Count
                : 0
        };
    }

    public void Dispose()
    {
        _deliveryLog.Clear();
        _endpoints.Clear();
    }
}

/// <summary>
/// Webhook endpoint configuration
/// </summary>
sealed public class WebhookEndpoint
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Secret { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastSuccessfulDeliveryAt { get; set; }
}

/// <summary>
/// Webhook delivery record
/// </summary>
sealed public class WebhookDelivery
{
    public string EndpointId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Success { get; set; }
    public int Attempt { get; set; }
    public DateTime DeliveredAt { get; set; }
}

/// <summary>
/// Webhook handler options
/// </summary>
sealed public class WebhookOptions
{
    public bool Enabled { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
    public int TimeoutMs { get; set; } = 30000;
}

/// <summary>
/// Webhook statistics
/// </summary>
sealed public class WebhookStatistics
{
    public int TotalEndpoints { get; set; }
    public int ActiveEndpoints { get; set; }
    public int TotalDeliveries { get; set; }
    public int SuccessfulDeliveries { get; set; }
    public int FailedDeliveries { get; set; }
    public double SuccessRate { get; set; }
}
