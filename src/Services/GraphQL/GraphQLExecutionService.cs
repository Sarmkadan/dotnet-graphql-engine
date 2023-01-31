#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Services.DataLoader; // Added for DataLoaderService
using Microsoft.Extensions.Logging;
using ExecutionContext = GraphQLEngine.Domain.Entities.ExecutionContext;

namespace GraphQLEngine.Services.GraphQL;

/// <summary>
/// Service for executing GraphQL queries and mutations.
/// </summary>
sealed public class GraphQLExecutionService
{
    private readonly ILogger<GraphQLExecutionService> _logger;
    private readonly DataLoaderService _dataLoaderService; // Injected DataLoaderService
    private readonly Dictionary<string, object> _resolvers = new();

    public GraphQLExecutionService(ILogger<GraphQLExecutionService> logger, DataLoaderService dataLoaderService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dataLoaderService = dataLoaderService ?? throw new ArgumentNullException(nameof(dataLoaderService));
    }

    /// <summary>
    /// Registers a resolver function for a field.
    /// </summary>
    /// <param name="fieldPath">The path to the field in the GraphQL schema.</param>
    /// <param name="resolver">The resolver function object.</param>
    public void RegisterResolver(string fieldPath, object resolver)
    {
        if (string.IsNullOrEmpty(fieldPath))
            throw new ArgumentException("Field path cannot be empty", nameof(fieldPath));

        if (resolver is null) throw new ArgumentNullException(nameof(resolver));

        _resolvers[fieldPath] = resolver;
        _logger.LogInformation("Resolver registered for field: {FieldPath}", fieldPath);
    }

    /// <summary>
    /// Executes a GraphQL query.
    /// </summary>
    /// <param name="query">The GraphQL query to execute.</param>
    /// <returns>The execution context containing results and errors.</returns>
    public async Task<ExecutionContext> ExecuteAsync(GraphQLQuery query)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));

        var context = new ExecutionContext(query.Id);
        // context.RequestedFieldCount = query.SelectedFields.Count; // Removed: will be calculated by QueryAnalysisService

        try
        {
            _logger.LogInformation("Starting execution of query: {QueryId}", query.Id);

            if (!query.Validate())
            {
                foreach (var error in query.Errors)
                    context.AddError(error);
                context.Complete();
                return context;
            }

            // Parse the query string into a hierarchical structure
            var rootSelections = ParseQuerySelections(query.QueryString);
            query.SetRootSelectedFields(rootSelections); // Populate the GraphQLQuery with structured fields

            // Now iterate through the structured fields for execution
            await ExecuteFieldsRecursiveAsync(rootSelections, context);

            // Ensure all data loader batches are flushed after all fields have been processed
            await _dataLoaderService.FlushAllAsync(context.Id);

            context.Complete();
            _logger.LogInformation("Query execution completed: {QueryId}, Duration: {Duration}ms",
                query.Id, context.DurationMs);

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query: {QueryId}", query.Id);
            context.Fail($"Execution failed: {ex.Message}");
            return context;
        }
    }

    /// <summary>
    /// Internal query execution logic
    /// </summary>
    private async Task<object?> ExecuteQueryInternalAsync(GraphQLQuery query, ExecutionContext context)
    {
        // Parse the query string into a hierarchical structure
        var rootSelections = ParseQuerySelections(query.QueryString);
        query.SetRootSelectedFields(rootSelections); // Populate the GraphQLQuery with structured fields

        // Now iterate through the structured fields for execution
        await ExecuteFieldsRecursiveAsync(rootSelections, context);

        return true;
    }

    /// <summary>
    /// Recursively executes fields based on the structured QueryField list,
    /// invoking any registered resolver for each field and storing its result
    /// in the execution context.
    /// </summary>
    private async Task ExecuteFieldsRecursiveAsync(IEnumerable<QueryField> fields, ExecutionContext context)
    {
        foreach (var field in fields)
        {
            try
            {
                var resolverKey = field.Name; // Use field name for resolver lookup
                if (_resolvers.TryGetValue(resolverKey, out var resolver))
                {
                    var result = await InvokeResolverAsync(resolver, context);
                    context.SetContextValue(field.Alias ?? field.Name, result);
                    context.RecordResolverExecution(field.Name);
                }
                else
                {
                    // No resolver registered – field resolves to null (standard GraphQL behaviour)
                    _logger.LogDebug("No resolver registered for field '{FieldName}'; returning null", field.Name);
                }

                // Recursively execute nested fields
                if (field.Fields.Any())
                {
                    await ExecuteFieldsRecursiveAsync(field.Fields, context);
                }
            }
            catch (Exception ex)
            {
                context.AddError(ex.Message, field.Name);
            }
        }
    }

    /// <summary>
    /// Invokes a registered resolver, supporting synchronous and asynchronous
    /// delegates with either no parameters or a single ExecutionContext parameter.
    /// </summary>
    private static async Task<object?> InvokeResolverAsync(object resolver, ExecutionContext context)
    {
        object? result = resolver switch
        {
            Func<ExecutionContext, Task<object?>> asyncWithContext => await asyncWithContext(context),
            Func<Task<object?>> asyncNoArgs => await asyncNoArgs(),
            Func<ExecutionContext, object?> syncWithContext => syncWithContext(context),
            Func<object?> syncNoArgs => syncNoArgs(),
            Delegate del => del.Method.GetParameters().Length == 1
                ? del.DynamicInvoke(context)
                : del.DynamicInvoke(),
            _ => resolver // A constant value registered as the resolver
        };

        // Unwrap Task/Task<T> results produced through the generic delegate path
        if (result is Task task)
        {
            await task;
            var resultProperty = task.GetType().GetProperty("Result");
            result = resultProperty is not null && resultProperty.PropertyType.Name != "VoidTaskResult"
                ? resultProperty.GetValue(task)
                : null;
        }

        return result;
    }

    /// <summary>
    /// Parses field selections from query string into a hierarchical QueryField structure.
    /// This is a basic implementation to support nested fields, aliases, and inline fragments.
    /// It does not fully support arguments, variables, or named fragments.
    /// </summary>
    private IReadOnlyList<QueryField> ParseQuerySelections(string queryString)
    {
        var fields = new List<QueryField>();
        var tokenizer = new QueryTokenizer(queryString);

        // Remove the outer query/mutation/subscription wrapper if present,
        // to get to the main selection set.
        var tokens = tokenizer.Tokenize().ToList();
        var startIndex = 0;
        var openBraceCount = 0;

        for (int i = 0; i < tokens.Count; i++)
        {
            if (tokens[i].Type == QueryTokenType.OpenBrace)
            {
                openBraceCount++;
                if (openBraceCount == 1) // First outer brace of the selection set
                {
                    startIndex = i + 1;
                    break;
                }
            }
        }

        if (startIndex == 0 && tokens.Any()) // No outer operation or root selection set
        {
            // Assume it's a direct selection set if no operation name and no outer braces
            // e.g. { user { id } }
            startIndex = tokens.FindIndex(t => t.Type == QueryTokenType.OpenBrace) + 1;
        }

        if (startIndex > 0 && startIndex < tokens.Count)
        {
            try
            {
                var parser = new QueryParser(tokens.Skip(startIndex).ToList());
                fields.AddRange(parser.ParseSelection());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing query selections: {QueryString}", queryString);
                // Optionally add error to context or throw a specific parsing exception
            }
        }
        
        return fields.AsReadOnly();
    }

    // --- Helper classes for parsing ---

    private enum QueryTokenType
    {
        Name,
        Alias,
        Colon,
        OpenBrace,
        CloseBrace,
        OpenParen,
        CloseParen,
        Spread, // ...
        On,     // on keyword for inline fragments
        String,
        Number,
        Boolean,
        Null,
        Comma,
        Whitespace,
        Unknown
    }

    private class QueryToken
    {
        public QueryTokenType Type { get; }
        public string Value { get; }

        public QueryToken(QueryTokenType type, string value)
        {
            Type = type;
            Value = value;
        }

        public override string ToString() => $"[{Type}] {Value}";
    }

    private class QueryTokenizer
    {
        private readonly string _query;
        private int _position;

        public QueryTokenizer(string query)
        {
            _query = query;
            _position = 0;
        }

        public IEnumerable<QueryToken> Tokenize()
        {
            while (_position < _query.Length)
            {
                if (char.IsWhiteSpace(_query[_position]))
                {
                    yield return ReadWhitespace();
                    continue;
                }

                switch (_query[_position])
                {
                    case '{':
                        yield return new QueryToken(QueryTokenType.OpenBrace, "{");
                        _position++;
                        break;
                    case '}':
                        yield return new QueryToken(QueryTokenType.CloseBrace, "}");
                        _position++;
                        break;
                    case '(':
                        yield return new QueryToken(QueryTokenType.OpenParen, "(");
                        _position++;
                        break;
                    case ')':
                        yield return new QueryToken(QueryTokenType.CloseParen, ")");
                        _position++;
                        break;
                    case ':':
                        yield return new QueryToken(QueryTokenType.Colon, ":");
                        _position++;
                        break;
                    case ',':
                        yield return new QueryToken(QueryTokenType.Comma, ",");
                        _position++;
                        break;
                    case '.':
                        if (_position + 2 < _query.Length && _query[_position + 1] == '.' && _query[_position + 2] == '.')
                        {
                            yield return new QueryToken(QueryTokenType.Spread, "...");
                            _position += 3;
                        }
                        else
                        {
                            yield return ReadName(); // Or handle as part of a number, or error
                        }
                        break;
                    case '"':
                        yield return ReadString();
                        break;
                    default:
                        if (char.IsLetter(_query[_position]) || _query[_position] == '_')
                        {
                            var nameToken = ReadName();
                            if (nameToken.Value.Equals("on", StringComparison.OrdinalIgnoreCase))
                            {
                                yield return new QueryToken(QueryTokenType.On, "on");
                            }
                            else if (nameToken.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                     nameToken.Value.Equals("false", StringComparison.OrdinalIgnoreCase))
                            {
                                yield return new QueryToken(QueryTokenType.Boolean, nameToken.Value);
                            }
                            else if (nameToken.Value.Equals("null", StringComparison.OrdinalIgnoreCase))
                            {
                                yield return new QueryToken(QueryTokenType.Null, nameToken.Value);
                            }
                            else
                            {
                                yield return nameToken;
                            }
                        }
                        else if (char.IsDigit(_query[_position]) || _query[_position] == '-')
                        {
                            yield return ReadNumber();
                        }
                        else
                        {
                            yield return new QueryToken(QueryTokenType.Unknown, _query[_position].ToString());
                            _position++;
                        }
                        break;
                }
            }
        }

        private QueryToken ReadWhitespace()
        {
            var start = _position;
            while (_position < _query.Length && char.IsWhiteSpace(_query[_position]))
            {
                _position++;
            }
            return new QueryToken(QueryTokenType.Whitespace, _query.Substring(start, _position - start));
        }

        private QueryToken ReadName()
        {
            var start = _position;
            while (_position < _query.Length && (char.IsLetterOrDigit(_query[_position]) || _query[_position] == '_'))
            {
                _position++;
            }
            return new QueryToken(QueryTokenType.Name, _query.Substring(start, _position - start));
        }

        private QueryToken ReadString()
        {
            var start = _position;
            _position++; // Skip opening quote
            while (_position < _query.Length && _query[_position] != '"')
            {
                if (_query[_position] == '\\' && _position + 1 < _query.Length)
                {
                    _position++; // Skip escaped character
                }
                _position++;
            }
            _position++; // Skip closing quote
            return new QueryToken(QueryTokenType.String, _query.Substring(start, _position - start));
        }

        private QueryToken ReadNumber()
        {
            var start = _position;
            while (_position < _query.Length && (char.IsDigit(_query[_position]) || _query[_position] == '-' || _query[_position] == '.'))
            {
                _position++;
            }
            return new QueryToken(QueryTokenType.Number, _query.Substring(start, _position - start));
        }
    }

    private class QueryParser
    {
        private readonly List<QueryToken> _tokens;
        private int _position;

        public QueryParser(List<QueryToken> tokens)
        {
            _tokens = tokens.Where(t => t.Type != QueryTokenType.Whitespace && t.Type != QueryTokenType.Comma).ToList();
            _position = 0;
        }

        private QueryToken Peek(int offset = 0)
        {
            if (_position + offset >= _tokens.Count)
            {
                return new QueryToken(QueryTokenType.Unknown, string.Empty);
            }
            return _tokens[_position + offset];
        }

        private QueryToken Consume()
        {
            if (_position >= _tokens.Count)
            {
                throw new InvalidOperationException("Unexpected end of query during parsing.");
            }
            return _tokens[_position++];
        }

        private void Expect(QueryTokenType type, string errorMessage)
        {
            var token = Consume();
            if (token.Type != type)
            {
                throw new InvalidOperationException(errorMessage + $" Got {token.Type} '{token.Value}'");
            }
        }

        public IReadOnlyList<QueryField> ParseSelection()
        {
            var fields = new List<QueryField>();

            // If we are called with a leading '{', consume it.
            // This allows ParseSelection to be called for root selection sets
            // or nested ones.
            if (Peek().Type == QueryTokenType.OpenBrace)
            {
                Consume(); // {
            }

            while (_position < _tokens.Count && Peek().Type != QueryTokenType.CloseBrace)
            {
                // Handle inline fragments: ... on TypeName { selection }
                if (Peek().Type == QueryTokenType.Spread)
                {
                    Consume(); // ...
                    Expect(QueryTokenType.On, "Expected 'on' keyword for inline fragment.");
                    var typeCondition = Consume(); // TypeName
                    if (typeCondition.Type != QueryTokenType.Name)
                    {
                        throw new InvalidOperationException($"Expected type name for inline fragment, got {typeCondition.Type}");
                    }
                    Expect(QueryTokenType.OpenBrace, "Expected '{' to open inline fragment selection set.");
                    var fragmentFields = ParseSelection(); // Recursively parse fragment selections
                    fields.Add(new QueryField(
                        name: "...", // Special name for fragment
                        typeCondition: typeCondition.Value,
                        fields: fragmentFields
                    ));
                }
                else // Regular field or aliased field
                {
                    var aliasOrName = Consume();
                    if (aliasOrName.Type != QueryTokenType.Name)
                    {
                        throw new InvalidOperationException($"Expected field name or alias, got {aliasOrName.Type}");
                    }

                    string fieldName;
                    string? alias = null;

                    if (Peek().Type == QueryTokenType.Colon) // Aliased field: alias: name { ... }
                    {
                        Consume(); // :
                        fieldName = Consume().Value;
                        alias = aliasOrName.Value;
                        if (fieldName == null)
                        {
                            throw new InvalidOperationException("Expected field name after alias.");
                        }
                    }
                    else // Regular field: name { ... }
                    {
                        fieldName = aliasOrName.Value;
                    }

                    // Skip arguments for now (if present, e.g., field(arg: "value"))
                    if (Peek().Type == QueryTokenType.OpenParen)
                    {
                        int parenCount = 0;
                        while (_position < _tokens.Count)
                        {
                            var token = Consume();
                            if (token.Type == QueryTokenType.OpenParen) parenCount++;
                            if (token.Type == QueryTokenType.CloseParen) parenCount--;
                            if (parenCount == 0 && token.Type == QueryTokenType.CloseParen) break;
                        }
                    }
                    
                    var nestedFields = new List<QueryField>();
                    if (Peek().Type == QueryTokenType.OpenBrace)
                    {
                        nestedFields.AddRange(ParseSelection()); // Recursively parse nested selections
                    }

                    fields.Add(new QueryField(
                        name: fieldName,
                        alias: alias,
                        fields: nestedFields
                    ));
                }
            }

            if (Peek().Type == QueryTokenType.CloseBrace)
            {
                Consume(); // }
            }

            return fields.AsReadOnly();
        }
    }

    /// <summary>
    /// Gets execution statistics.
    /// </summary>
    /// <returns>A dictionary containing execution statistics.</returns>
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "RegisteredResolvers", _resolvers.Count },
            { "Timestamp", DateTime.UtcNow }
        };
    }
}
