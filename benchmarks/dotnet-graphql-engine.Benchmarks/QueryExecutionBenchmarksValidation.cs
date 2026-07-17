namespace benchmarks.dotnet_graphql_engine.Benchmarks;

/// <summary>
/// Provides validation methods for <see cref="QueryExecutionBenchmarks"/> instances to ensure benchmark configurations are properly defined.
/// </summary>
public static class QueryExecutionBenchmarksValidation
{
    /// <summary>
    /// Validates the specified <paramref name="value"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The <see cref="QueryExecutionBenchmarks"/> instance to validate.</param>
    /// <returns>A list of problems with the <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this QueryExecutionBenchmarks? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.Setup))
        {
            problems.Add("Setup method name is null, empty, or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(value.Cleanup))
        {
            problems.Add("Cleanup method name is null, empty, or whitespace.");
        }

        if (value.SimpleQuery is null)
        {
            problems.Add("SimpleQuery property is null.");
        }

        if (value.NestedQuery is null)
        {
            problems.Add("NestedQuery property is null.");
        }

        if (value.ComplexQuery is null)
        {
            problems.Add("ComplexQuery property is null.");
        }

        if (value.LargeQuery is null)
        {
            problems.Add("LargeQuery property is null.");
        }

        if (value.IntrospectionQuery is null)
        {
            problems.Add("IntrospectionQuery property is null.");
        }

        if (value.MultipleSimpleQueries is null)
        {
            problems.Add("MultipleSimpleQueries property is null.");
        }

        if (value.CreateSchema is null)
        {
            problems.Add("CreateSchema method is null.");
        }

        if (value.RegisterResolver is null)
        {
            problems.Add("RegisterResolver method is null.");
        }

        if (value.QueryWithArguments is null)
        {
            problems.Add("QueryWithArguments method is null.");
        }

        return problems;
    }

    /// <summary>
    /// Determines whether the specified <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The <see cref="QueryExecutionBenchmarks"/> instance to validate.</param>
    /// <returns><c>true</c> if the <paramref name="value"/> is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(this QueryExecutionBenchmarks? value) => Validate(value).Count == 0;

    /// <summary>
    /// Ensures that the specified <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The <see cref="QueryExecutionBenchmarks"/> instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="value"/> is not valid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static void EnsureValid(this QueryExecutionBenchmarks? value)
    {
        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException($"Invalid QueryExecutionBenchmarks: {string.Join(Environment.NewLine, problems)}", nameof(value));
        }
    }
}