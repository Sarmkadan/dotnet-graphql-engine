#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Domain.Entities;

/// <summary>
/// Represents a GraphQL mutation operation
/// </summary>
sealed public class GraphQLMutation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string MutationString { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long ExecutionTimeMs { get; set; } = 0;
    public MutationState State { get; set; } = MutationState.Pending;

    private readonly Dictionary<string, object?> _variables = new();
    public IReadOnlyDictionary<string, object?> Variables => _variables.AsReadOnly();

    private readonly List<string> _affectedFields = new();
    public IReadOnlyList<string> AffectedFields => _affectedFields.AsReadOnly();

    private readonly List<string> _errors = new();
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    private Dictionary<string, object?>? _result;
    public Dictionary<string, object?>? Result => _result;

    public GraphQLMutation()
    {
    }

    public GraphQLMutation(string name, string mutationString)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        MutationString = mutationString ?? throw new ArgumentNullException(nameof(mutationString));
    }

    /// <summary>
    /// Sets a variable for mutation execution
    /// </summary>
    public void SetVariable(string name, object? value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Variable name cannot be empty", nameof(name));

        _variables[name] = value;
    }

    /// <summary>
    /// Gets a variable value
    /// </summary>
    public object? GetVariable(string name)
    {
        _variables.TryGetValue(name, out var value);
        return value;
    }

    /// <summary>
    /// Adds a field that will be affected by this mutation
    /// </summary>
    public void AddAffectedField(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return;

        if (!_affectedFields.Contains(fieldName))
            _affectedFields.Add(fieldName);
    }

    /// <summary>
    /// Gets all affected fields
    /// </summary>
    public IEnumerable<string> GetAffectedFields()
    {
        return _affectedFields.AsReadOnly();
    }

    /// <summary>
    /// Adds an error
    /// </summary>
    public void AddError(string error)
    {
        if (string.IsNullOrEmpty(error)) return;

        _errors.Add(error);
        State = MutationState.Failed;
    }

    /// <summary>
    /// Sets the result of the mutation
    /// </summary>
    public void SetResult(Dictionary<string, object?> result)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
        State = MutationState.Completed;
    }

    /// <summary>
    /// Validates the mutation
    /// </summary>
    public bool Validate(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Mutation name is required");

        if (string.IsNullOrWhiteSpace(MutationString))
            errors.Add("Mutation string is required");

        if (MutationString.Length > 100000)
            errors.Add("Mutation exceeds maximum length");

        return errors.Count == 0;
    }

    /// <summary>
    /// Checks if the mutation is a create operation
    /// </summary>
    public bool IsCreateOperation()
    {
        return Name.StartsWith("create", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the mutation is an update operation
    /// </summary>
    public bool IsUpdateOperation()
    {
        return Name.StartsWith("update", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the mutation is a delete operation
    /// </summary>
    public bool IsDeleteOperation()
    {
        return Name.StartsWith("delete", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets a summary of the mutation
    /// </summary>
    public string GetSummary()
    {
        return $"Mutation '{Name}' (ID: {Id}): " +
               $"State: {State}, Duration: {ExecutionTimeMs}ms, " +
               $"Affected fields: {_affectedFields.Count}, Errors: {_errors.Count}";
    }
}

/// <summary>
/// Enumeration of mutation states
/// </summary>
public enum MutationState
{
    Pending = 0,
    Executing = 1,
    Completed = 2,
    Failed = 3,
    Rolled = 4
}
