#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GraphQLEngine.Api.Middleware;

/// <summary>
/// Authentication middleware that validates incoming requests
/// Supports API key and Bearer token authentication
/// </summary>
sealed public class AuthenticationMiddleware
{
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly AuthenticationOptions _options;
    private readonly Dictionary<string, string> _apiKeys;

    public AuthenticationMiddleware(
        ILogger<AuthenticationMiddleware> logger,
        AuthenticationOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new AuthenticationOptions();
        _apiKeys = new Dictionary<string, string>();

        // Initialize with default API keys (should be loaded from config)
        InitializeApiKeys();
    }

    /// <summary>
    /// Authenticates a request and extracts principal information
    /// Returns null if authentication fails
    /// </summary>
    public AuthenticationResult AuthenticateRequest(
        Dictionary<string, string> headers,
        Dictionary<string, string> queryParameters)
    {
        try
        {
            // Check API key in header
            if (headers.ContainsKey("X-API-Key"))
            {
                var apiKey = headers["X-API-Key"];
                if (ValidateApiKey(apiKey))
                {
                    _logger.LogInformation("Request authenticated with API key");
                    return new AuthenticationResult
                    {
                        Success = true,
                        Principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, apiKey),
                            new Claim("auth_type", "api_key")
                        }))
                    };
                }
            }

            // Check Bearer token in Authorization header
            if (headers.ContainsKey("Authorization"))
            {
                var authHeader = headers["Authorization"];
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    if (ValidateToken(token))
                    {
                        _logger.LogInformation("Request authenticated with Bearer token");
                        return new AuthenticationResult
                        {
                            Success = true,
                            Principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
                            {
                                new Claim(ClaimTypes.NameIdentifier, ExtractUserId(token)),
                                new Claim("auth_type", "bearer_token")
                            }))
                        };
                    }
                }
            }

            // Check query parameter (less secure, for development)
            if (_options.AllowQueryParameterAuth && queryParameters.ContainsKey("api_key"))
            {
                var apiKey = queryParameters["api_key"];
                if (ValidateApiKey(apiKey))
                {
                    _logger.LogWarning("Request authenticated with query parameter API key (less secure)");
                    return new AuthenticationResult
                    {
                        Success = true,
                        Principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, apiKey),
                            new Claim("auth_type", "api_key")
                        }))
                    };
                }
            }

            _logger.LogWarning("Authentication failed: no valid credentials provided");

            return new AuthenticationResult
            {
                Success = false,
                Error = "No valid authentication credentials provided"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            return new AuthenticationResult
            {
                Success = false,
                Error = "Authentication error"
            };
        }
    }

    /// <summary>
    /// Validates an API key against the stored keys
    /// </summary>
    private bool ValidateApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return false;

        // In production, this would check against a database or secure storage
        return _apiKeys.ContainsKey(apiKey);
    }

    /// <summary>
    /// Validates a JWT: structural check, signature verification (when a secret is
    /// configured), issuer/audience match and expiration
    /// </summary>
    private bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var parts = token.Split('.');
        if (parts.Length != 3)
            return false;

        var payload = DecodeJwtPart(parts[1]);
        if (payload is null)
            return false;

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            // Expiration check ("exp" is seconds since Unix epoch)
            if (root.TryGetProperty("exp", out var exp) && exp.TryGetInt64(out var expSeconds))
            {
                if (DateTimeOffset.FromUnixTimeSeconds(expSeconds) <= DateTimeOffset.UtcNow)
                {
                    _logger.LogWarning("Token rejected: expired");
                    return false;
                }
            }

            // Not-before check
            if (root.TryGetProperty("nbf", out var nbf) && nbf.TryGetInt64(out var nbfSeconds))
            {
                if (DateTimeOffset.FromUnixTimeSeconds(nbfSeconds) > DateTimeOffset.UtcNow)
                {
                    _logger.LogWarning("Token rejected: not yet valid");
                    return false;
                }
            }

            // Issuer / audience checks when configured
            if (!string.IsNullOrEmpty(_options.JwtIssuer))
            {
                if (!root.TryGetProperty("iss", out var iss) ||
                    !string.Equals(iss.GetString(), _options.JwtIssuer, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Token rejected: issuer mismatch");
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(_options.JwtAudience))
            {
                if (!root.TryGetProperty("aud", out var aud) ||
                    !string.Equals(aud.GetString(), _options.JwtAudience, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Token rejected: audience mismatch");
                    return false;
                }
            }
        }
        catch (JsonException)
        {
            return false;
        }

        // HMAC-SHA256 signature verification when a secret is configured
        if (!string.IsNullOrEmpty(_options.JwtSecret))
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.JwtSecret));
            var signedData = Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}");
            var expected = hmac.ComputeHash(signedData);

            var actual = Base64UrlDecode(parts[2]);
            if (actual is null || !CryptographicOperations.FixedTimeEquals(expected, actual))
            {
                _logger.LogWarning("Token rejected: signature verification failed");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Extracts the user ID from the JWT payload ("sub", "nameid" or "user_id" claim)
    /// </summary>
    private static string ExtractUserId(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
            return "unknown_user";

        var payload = DecodeJwtPart(parts[1]);
        if (payload is null)
            return "unknown_user";

        try
        {
            using var doc = JsonDocument.Parse(payload);
            foreach (var claim in new[] { "sub", "nameid", "user_id" })
            {
                if (doc.RootElement.TryGetProperty(claim, out var value))
                {
                    var id = value.ValueKind == JsonValueKind.String
                        ? value.GetString()
                        : value.GetRawText();
                    if (!string.IsNullOrWhiteSpace(id))
                        return id;
                }
            }
        }
        catch (JsonException)
        {
            // Fall through to the default
        }

        return "unknown_user";
    }

    /// <summary>
    /// Decodes a base64url-encoded JWT segment into a UTF-8 string
    /// </summary>
    private static string? DecodeJwtPart(string part)
    {
        var bytes = Base64UrlDecode(part);
        return bytes is null ? null : Encoding.UTF8.GetString(bytes);
    }

    private static byte[]? Base64UrlDecode(string input)
    {
        if (string.IsNullOrEmpty(input))
            return null;

        var base64 = input.Replace('-', '+').Replace('_', '/');
        base64 = (base64.Length % 4) switch
        {
            2 => base64 + "==",
            3 => base64 + "=",
            1 => null!,
            _ => base64
        };

        if (base64 is null)
            return null;

        try
        {
            return Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            return null;
        }
    }

    /// <summary>
    /// Initializes the API keys dictionary from the configured options,
    /// falling back to development keys when none are configured
    /// </summary>
    private void InitializeApiKeys()
    {
        if (_options.ApiKeys.Count > 0)
        {
            foreach (var (key, description) in _options.ApiKeys)
                _apiKeys[key] = description;
            return;
        }

        // Development fallback keys used only when no keys are configured
        _apiKeys["dev-key-123"] = "Development API Key";
        _apiKeys["test-key-456"] = "Test API Key";
    }
}

/// <summary>
/// Authentication configuration options
/// </summary>
sealed public class AuthenticationOptions
{
    public bool RequireAuthentication { get; set; } = true;
    public bool AllowQueryParameterAuth { get; set; } = false;
    public bool AllowAnonymousIntrospection { get; set; } = true;
    public int TokenExpirationMinutes { get; set; } = 60;
    public string? JwtSecret { get; set; }
    public string? JwtIssuer { get; set; }
    public string? JwtAudience { get; set; }
    public Dictionary<string, string> ApiKeys { get; set; } = new();
}

/// <summary>
/// Result of an authentication attempt
/// </summary>
sealed public class AuthenticationResult
{
    public bool Success { get; set; }
    public ClaimsPrincipal? Principal { get; set; }
    public string? Error { get; set; }
    public DateTime AuthenticatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Authenticated context information
/// </summary>
sealed public class AuthenticationContext
{
    public bool IsAuthenticated { get; set; }
    public ClaimsPrincipal? Principal { get; set; }
    public string? UserId { get; set; }
    public List<string> Roles { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
