// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Text;

namespace GraphQLEngine.Api.Middleware;

/// <summary>
/// Authentication middleware that validates incoming requests
/// Supports API key and Bearer token authentication
/// </summary>
public class AuthenticationMiddleware
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
                    return new AuthenticationResult { Success = true };
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
    /// Validates a JWT or similar token
    /// </summary>
    private bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        // Simplified token validation (should use JWT library in production)
        try
        {
            var parts = token.Split('.');
            return parts.Length == 3; // Basic JWT structure check
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts user ID from the token
    /// </summary>
    private string ExtractUserId(string token)
    {
        // Simplified extraction (would parse JWT in production)
        return "unknown_user";
    }

    /// <summary>
    /// Initializes the API keys dictionary
    /// In production, this would load from configuration or secure storage
    /// </summary>
    private void InitializeApiKeys()
    {
        // Add some default keys for development
        _apiKeys["dev-key-123"] = "Development API Key";
        _apiKeys["test-key-456"] = "Test API Key";
    }
}

/// <summary>
/// Authentication configuration options
/// </summary>
public class AuthenticationOptions
{
    public bool RequireAuthentication { get; set; } = true;
    public bool AllowQueryParameterAuth { get; set; } = false;
    public bool AllowAnonymousIntrospection { get; set; } = true;
    public int TokenExpirationMinutes { get; set; } = 60;
    public string? JwtSecret { get; set; }
    public string? JwtIssuer { get; set; }
    public string? JwtAudience { get; set; }
}

/// <summary>
/// Result of an authentication attempt
/// </summary>
public class AuthenticationResult
{
    public bool Success { get; set; }
    public ClaimsPrincipal? Principal { get; set; }
    public string? Error { get; set; }
    public DateTime AuthenticatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Authenticated context information
/// </summary>
public class AuthenticationContext
{
    public bool IsAuthenticated { get; set; }
    public ClaimsPrincipal? Principal { get; set; }
    public string? UserId { get; set; }
    public List<string> Roles { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
