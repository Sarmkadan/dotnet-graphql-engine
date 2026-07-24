#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Common.Constants;
using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.Caching;
using Microsoft.Extensions.Logging;
using System.Linq; // Added for LINQ extensions

namespace GraphQLEngine.Services.QueryAnalysis;

/// <summary>
/// Service for analyzing GraphQL query complexity
/// </summary>
sealed public class QueryAnalysisService
{
    private readonly ILogger<QueryAnalysisService> _logger;

    // Bounded analysis cache keyed by query id. Analyses derive from parsed query
    // documents, so this store is capped and evicts on an LRU basis to prevent
    // unbounded memory growth when callers submit unlimited unique queries.
    private readonly ICacheStore<string, QueryComplexity> _analyses;

    /// <summary>
    /// Initialises the service with the required logger.
    /// </summary>
    /// <param name="logger">Diagnostic logger.</param>
    /// <param name="analysisCache">
    /// Optional cache store for analyzed queries. When omitted, a
    /// <see cref="LruCacheStore{TKey, TValue}"/> bounded by
    /// <see cref="GraphQLConstants.DocumentCacheMaxEntries"/> is used.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public QueryAnalysisService(
        ILogger<QueryAnalysisService> logger,
        ICacheStore<string, QueryComplexity>? analysisCache = null)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _analyses = analysisCache ?? new LruCacheStore<string, QueryComplexity>(
            GraphQLConstants.DocumentCacheMaxEntries);
    }

    /// <summary>
    /// Analyzes the complexity of a GraphQL query
    /// </summary>
    public QueryComplexity AnalyzeQuery(GraphQLQuery query)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));

        var analysis = new QueryComplexity(query.Id);

        try
        {
            // Use pre-parsed fields if available; otherwise parse the raw query string.
            var fields = query.RootSelectedFields.Count > 0
                ? query.RootSelectedFields
                : ParseQueryStringToFields(query.QueryString);

            // Calculate max depth and field count from the structured query fields
            analysis.MaxDepth = CalculateMaxDepth(fields);
            analysis.FieldCount = CalculateFieldCount(fields);

            // Calculate complexity score using the hierarchical structure
            CalculateComplexityScore(fields, analysis, 1);

            // Determine complexity level
            analysis.CalculateLevel();

            // Add warnings for high complexity
            if (analysis.Level == QueryComplexityLevel.Critical)
            {
                analysis.AddWarning("Query complexity is critical. Consider breaking it into smaller queries.");
            }
            else if (analysis.Level == QueryComplexityLevel.High)
            {
                analysis.AddWarning("Query complexity is high. Consider optimizing.");
            }

            _analyses.Set(query.Id, analysis);

            _logger.LogInformation("Query analysis completed: {QueryId}, Score: {Score}, Level: {Level}",
                query.Id, analysis.TotalScore, analysis.Level);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query: {QueryId}", query.Id);
            analysis.AddWarning($"Analysis failed: {ex.Message}");
            return analysis;
        }
    }

    /// <summary>
    /// Recursively calculates the complexity score for a list of QueryFields.
    /// </summary>
    /// <param name="fields">The list of query fields to analyze.</param>
    /// <param name="analysis">The QueryComplexity instance to update.</param>
    /// <param name="depth">The current depth in the query tree.</param>
    private void CalculateComplexityScore(IReadOnlyList<QueryField> fields, QueryComplexity analysis, int depth)
    {
        foreach (var field in fields)
        {
            // Base complexity for each field
            var fieldComplexity = 1; 

            // Record complexity for the current field
            analysis.RecordFieldComplexity(field.Name, fieldComplexity);

            // Recursively calculate complexity for nested fields
            if (field.Fields.Any())
            {
                CalculateComplexityScore(field.Fields, analysis, depth + 1);
            }
        }
    }

    /// <summary>
    /// Recursively calculates the maximum depth of the query.
    /// </summary>
    private int CalculateMaxDepth(IReadOnlyList<QueryField> fields, int currentDepth = 0)
    {
        if (!fields.Any())
        {
            return currentDepth;
        }

        return fields.Max(f => CalculateMaxDepth(f.Fields, currentDepth + 1));
    }

    /// <summary>
    /// Recursively calculates the total number of selected fields.
    /// </summary>
    private int CalculateFieldCount(IReadOnlyList<QueryField> fields)
    {
        if (!fields.Any())
        {
            return 0;
        }

        return fields.Count + fields.Sum(f => CalculateFieldCount(f.Fields));
    }

    // -------------------------------------------------------------------------
    // Minimal recursive-descent parser used when RootSelectedFields is empty.
    // Supports field names, aliases, arguments, directives, and inline fragments.
    // -------------------------------------------------------------------------

    private static IReadOnlyList<QueryField> ParseQueryStringToFields(string queryString)
    {
        if (string.IsNullOrWhiteSpace(queryString))
            return Array.Empty<QueryField>();

        var pos = 0;
        SkipWs(queryString, ref pos);

        // Skip optional operation keyword: query / mutation / subscription
        if (pos < queryString.Length && char.IsLetter(queryString[pos]))
        {
            var keyword = ReadIdent(queryString, ref pos);
            if (keyword is "query" or "mutation" or "subscription")
            {
                SkipWs(queryString, ref pos);
                // Skip optional operation name
                if (pos < queryString.Length && (char.IsLetter(queryString[pos]) || queryString[pos] == '_'))
                {
                    ReadIdent(queryString, ref pos);
                    SkipWs(queryString, ref pos);
                }
                // Skip optional variable definitions
                if (pos < queryString.Length && queryString[pos] == '(')
                    SkipBalanced(queryString, ref pos, '(', ')');
            }
        }

        if (pos >= queryString.Length || queryString[pos] != '{')
            return Array.Empty<QueryField>();

        pos++; // consume '{'
        return ParseSelectionSet(queryString, ref pos);
    }

    private static IReadOnlyList<QueryField> ParseSelectionSet(string s, ref int pos)
    {
        var fields = new List<QueryField>();

        while (pos < s.Length)
        {
            SkipWs(s, ref pos);
            if (pos >= s.Length) break;

            var ch = s[pos];
            if (ch == '}') { pos++; break; }

            // Skip line comments
            if (ch == '#') { while (pos < s.Length && s[pos] != '\n') pos++; continue; }

            // Inline fragment: ... on TypeName { ... }  or named fragment spread: ...FragName
            if (pos + 2 < s.Length && ch == '.' && s[pos + 1] == '.' && s[pos + 2] == '.')
            {
                pos += 3;
                SkipWs(s, ref pos);
                if (pos + 1 < s.Length && s.Substring(pos, Math.Min(2, s.Length - pos)) == "on"
                    && (pos + 2 >= s.Length || !char.IsLetterOrDigit(s[pos + 2])))
                {
                    pos += 2;
                    SkipWs(s, ref pos);
                    ReadIdent(s, ref pos); // type condition name
                    SkipWs(s, ref pos);
                    if (pos < s.Length && s[pos] == '{') { pos++; var ff = ParseSelectionSet(s, ref pos); foreach (var f in ff) fields.Add(f); }
                }
                else
                {
                    // Named fragment spread – just skip the name
                    if (pos < s.Length && (char.IsLetter(s[pos]) || s[pos] == '_'))
                        ReadIdent(s, ref pos);
                }
                continue;
            }

            if (!char.IsLetter(ch) && ch != '_') { pos++; continue; }

            var nameOrAlias = ReadIdent(s, ref pos);
            SkipWs(s, ref pos);

            string fieldName;
            if (pos < s.Length && s[pos] == ':')
            {
                pos++; SkipWs(s, ref pos);
                fieldName = pos < s.Length ? ReadIdent(s, ref pos) : nameOrAlias;
                SkipWs(s, ref pos);
            }
            else
            {
                fieldName = nameOrAlias;
            }

            // Skip arguments
            if (pos < s.Length && s[pos] == '(') SkipBalanced(s, ref pos, '(', ')');
            SkipWs(s, ref pos);

            // Skip directives (@directive(args))
            while (pos < s.Length && s[pos] == '@')
            {
                pos++; ReadIdent(s, ref pos); SkipWs(s, ref pos);
                if (pos < s.Length && s[pos] == '(') SkipBalanced(s, ref pos, '(', ')');
                SkipWs(s, ref pos);
            }

            IReadOnlyList<QueryField> nested = Array.Empty<QueryField>();
            if (pos < s.Length && s[pos] == '{') { pos++; nested = ParseSelectionSet(s, ref pos); }

            fields.Add(new QueryField(fieldName, fields: nested));
        }

        return fields.AsReadOnly();
    }

    private static string ReadIdent(string s, ref int pos)
    {
        var start = pos;
        while (pos < s.Length && (char.IsLetterOrDigit(s[pos]) || s[pos] == '_')) pos++;
        return s.Substring(start, pos - start);
    }

    private static void SkipWs(string s, ref int pos)
    {
        while (pos < s.Length && (char.IsWhiteSpace(s[pos]) || s[pos] == ',')) pos++;
    }

    private static void SkipBalanced(string s, ref int pos, char open, char close)
    {
        if (pos >= s.Length || s[pos] != open) return;
        var depth = 0;
        while (pos < s.Length)
        {
            if (s[pos] == open) depth++;
            else if (s[pos] == close) { depth--; if (depth == 0) { pos++; return; } }
            pos++;
        }
    }

    /// <summary>
    /// Gets a previous analysis by query ID
    /// </summary>
    public QueryComplexity? GetAnalysis(string queryId)
    {
        _analyses.TryGet(queryId, out var analysis);
        return analysis;
    }

    /// <summary>
    /// Checks if a query should be allowed based on complexity
    /// </summary>
    public bool IsQueryAllowed(GraphQLQuery query, int maxComplexityScore = 5000)
    {
        var analysis = AnalyzeQuery(query);
        return analysis.IsAcceptable(maxComplexityScore);
    }

    /// <summary>
    /// Gets a detailed complexity report
    /// </summary>
    public string GetComplexityReport(string queryId)
    {
        var analysis = GetAnalysis(queryId);
        if (analysis is null)
            return $"No analysis found for query: {queryId}";

        return analysis.GetReport();
    }

    /// <summary>
    /// Gets statistics for all analyzed queries
    /// </summary>
    public Dictionary<string, object> GetStatistics()
    {
        var snapshot = _analyses.Snapshot();
        var totalScore = snapshot.Sum(a => a.TotalScore);
        var avgScore = snapshot.Count > 0 ? totalScore / snapshot.Count : 0;

        var complexityDistribution = new Dictionary<string, int>
        {
            { "Low", snapshot.Count(a => a.Level == QueryComplexityLevel.Low) },
            { "Medium", snapshot.Count(a => a.Level == QueryComplexityLevel.Medium) },
            { "High", snapshot.Count(a => a.Level == QueryComplexityLevel.High) },
            { "Critical", snapshot.Count(a => a.Level == QueryComplexityLevel.Critical) }
        };

        return new Dictionary<string, object>
        {
            { "TotalQueriesAnalyzed", snapshot.Count },
            { "AverageComplexityScore", avgScore },
            { "MaxComplexityScore", snapshot.Count > 0 ? snapshot.Max(a => a.TotalScore) : 0 },
            { "ComplexityDistribution", complexityDistribution },
            { "MaxCacheEntries", _analyses.MaxEntries },
            { "Timestamp", DateTime.UtcNow }
        };
    }

    /// <summary>
    /// Clears analyses older than <paramref name="daysOld"/> days.
    /// </summary>
    /// <param name="daysOld">Age threshold in days; analyses older than this are evicted.</param>
    /// <returns>The number of analyses removed.</returns>
    public int ClearOldAnalyses(int daysOld = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var keysToRemove = _analyses
            .Snapshot()
            .Where(a => a.AnalyzedAt < cutoffDate)
            .Select(a => a.QueryId)
            .ToList();

        foreach (var key in keysToRemove)
            _analyses.Remove(key);

        _logger.LogInformation("Cleared {Count} old query analyses", keysToRemove.Count);
        return keysToRemove.Count;
    }
}
