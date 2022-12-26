#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Common.Constants;
using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Middleware;

/// <summary>
/// Protects the server from deeply-nested GraphQL queries that can cause excessive
/// resource consumption. The limiter measures nesting depth by counting selection-set
/// braces in the raw query document and rejects any query whose depth exceeds
/// <see cref="QueryDepthLimiterOptions.MaxDepth"/>.
/// </summary>
/// <example>
/// Register and use the limiter before executing a query:
/// <code>
/// var limiter = new QueryDepthLimiter(logger);
/// var result = limiter.Check(queryString);
/// if (!result.Allowed)
/// {
///     // return HTTP 400 with result.ToErrorJson()
/// }
/// </code>
/// </example>
sealed public class QueryDepthLimiter
{
    private readonly ILogger<QueryDepthLimiter> _logger;
    private readonly QueryDepthLimiterOptions _options;

    /// <summary>
    /// Initialises the limiter with the supplied logger and options.
    /// </summary>
    public QueryDepthLimiter(
        ILogger<QueryDepthLimiter> logger,
        QueryDepthLimiterOptions? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new QueryDepthLimiterOptions();
    }

    /// <summary>
    /// Calculates the nesting depth of <paramref name="query"/> and returns a result
    /// indicating whether the query is within the configured limit.
    /// </summary>
    /// <param name="query">Raw GraphQL document string.</param>
    /// <returns>
    /// A <see cref="DepthCheckResult"/> describing the measured depth and whether the
    /// query is allowed. When the depth exceeds the limit,
    /// <see cref="DepthCheckResult.Allowed"/> is <c>false</c> and
    /// <see cref="DepthCheckResult.ErrorMessage"/> is populated with a human-readable
    /// explanation suitable for returning to the caller as an HTTP 400 response.
    /// </returns>
    public DepthCheckResult Check(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new DepthCheckResult
            {
                Allowed = false,
                Depth = 0,
                MaxDepth = _options.MaxDepth,
                ErrorMessage = "Query cannot be empty"
            };
        }

        var depth = MeasureDepth(query);

        _logger.LogDebug("Query depth measured at {Depth} (limit {MaxDepth})", depth, _options.MaxDepth);

        if (depth > _options.MaxDepth)
        {
            var message = $"Query depth {depth} exceeds maximum of {_options.MaxDepth}";
            _logger.LogWarning("{Message}", message);

            return new DepthCheckResult
            {
                Allowed = false,
                Depth = depth,
                MaxDepth = _options.MaxDepth,
                ErrorMessage = message
            };
        }

        return new DepthCheckResult
        {
            Allowed = true,
            Depth = depth,
            MaxDepth = _options.MaxDepth
        };
    }

    /// <summary>
    /// Walks the raw query string character-by-character and returns the maximum
    /// selection-set nesting depth, measured by the deepest level of <c>{ }</c> pairs
    /// found outside string literals and comments.
    /// </summary>
    private static int MeasureDepth(string query)
    {
        int currentDepth = 0;
        int maxDepth = 0;
        bool inString = false;
        bool inComment = false;
        int length = query.Length;

        for (int i = 0; i < length; i++)
        {
            char c = query[i];

            // Single-line comment — skip to end of line
            if (!inString && c == '#')
            {
                inComment = true;
                continue;
            }

            if (inComment)
            {
                if (c == '\n')
                    inComment = false;
                continue;
            }

            // String literal — skip its contents (handles escape sequences)
            if (c == '"' && !inString)
            {
                // Block string (""" ... """)
                if (i + 2 < length && query[i + 1] == '"' && query[i + 2] == '"')
                {
                    i += 3; // skip opening """
                    while (i < length)
                    {
                        if (query[i] == '"' && i + 2 < length && query[i + 1] == '"' && query[i + 2] == '"')
                        {
                            i += 2; // skip closing """
                            break;
                        }
                        i++;
                    }
                }
                else
                {
                    inString = true;
                }
                continue;
            }

            if (inString)
            {
                if (c == '\\') { i++; continue; } // escape sequence
                if (c == '"') inString = false;
                continue;
            }

            if (c == '{')
            {
                currentDepth++;
                if (currentDepth > maxDepth)
                    maxDepth = currentDepth;
            }
            else if (c == '}')
            {
                currentDepth--;
            }
        }

        return maxDepth;
    }
}

/// <summary>
/// Configuration options for <see cref="QueryDepthLimiter"/>.
/// </summary>
sealed public class QueryDepthLimiterOptions
{
    /// <summary>
    /// Maximum allowed nesting depth for incoming GraphQL queries.
    /// Queries with a measured depth strictly greater than this value are rejected.
    /// Defaults to <see cref="GraphQLConstants.DefaultMaxQueryDepth"/> (10).
    /// </summary>
    public int MaxDepth { get; set; } = GraphQLConstants.DefaultMaxQueryDepth;
}

/// <summary>
/// Result produced by <see cref="QueryDepthLimiter.Check"/>.
/// </summary>
sealed public class DepthCheckResult
{
    /// <summary><c>true</c> when the query depth is within the configured limit.</summary>
    public bool Allowed { get; init; }

    /// <summary>Measured nesting depth of the submitted query.</summary>
    public int Depth { get; init; }

    /// <summary>The configured maximum depth at the time of the check.</summary>
    public int MaxDepth { get; init; }

    /// <summary>
    /// Human-readable rejection reason when <see cref="Allowed"/> is <c>false</c>.
    /// Returns <c>null</c> when the query is allowed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Serialises the rejection error into the GraphQL error-response format expected by
    /// clients, e.g. <c>{"errors":[{"message":"Query depth 15 exceeds maximum of 10"}]}</c>.
    /// Returns <c>null</c> when the query is allowed.
    /// </summary>
    public string? ToErrorJson()
    {
        if (Allowed || ErrorMessage is null)
            return null;

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            errors = new[] { new { message = ErrorMessage } }
        });
    }
}
