# QueryField

`QueryField` represents a node in a GraphQL query AST that describes a field to be resolved, including its name, alias, type condition, arguments, and nested fields. It is used to construct executable GraphQL queries programmatically in C#.

## API

### `Name`
- **Purpose**: Gets the name of the field as it appears in the schema.
- **Return value**: A non-null string representing the field name.
- **Exceptions**: Throws if the field name is null or empty during construction.

### `Alias`
- **Purpose**: Gets the optional alias applied to the field in the query.
- **Return value**: A nullable string representing the alias, or null if none is set.
- **Notes**: When present, the alias overrides the field name in the generated query.

### `TypeCondition`
- **Purpose**: Gets the optional GraphQL type condition (e.g., fragment spread) applied to the field.
- **Return value**: A nullable string representing the type condition, or null if none is set.
- **Notes**: Used to constrain the field to a specific type in the schema.

### `Arguments`
- **Purpose**: Gets the list of arguments applied to the field.
- **Return value**: An `IReadOnlyList<QueryArgument>` containing zero or more arguments.
- **Notes**: Arguments are used to parameterize the field during query execution.

### `Fields`
- **Purpose**: Gets the list of nested fields under this field.
- **Return value**: An `IReadOnlyList<QueryField>` containing zero or more child fields.
- **Notes**: Used to construct hierarchical queries with nested selections.

### `Value`
- **Purpose**: Gets the constant value assigned to this field, if any.
- **Return value**: An `object?` representing the value, or null if none is set.
- **Notes**: Used for scalar or literal values in queries.

## Usage

```csharp
// Example 1: Constructing a simple query with an argument
var query = new QueryField("user")
{
    Arguments = new List<QueryArgument>
    {
        new QueryArgument("id", "123")
    },
    Fields = new List<QueryField>
    {
        new QueryField("name"),
        new QueryField("email")
    }
};

// Example 2: Using an alias and type condition
var fragmentQuery = new QueryField("node")
{
    Alias = "userNode",
    TypeCondition = "User",
    Fields = new List<QueryField>
    {
        new QueryField("id"),
        new QueryField("profile")
        {
            Fields = new List<QueryField>
            {
                new QueryField("displayName")
            }
        }
    }
};
```

## Notes

- Thread safety: `QueryField` is immutable once constructed (assuming `QueryArgument` and child collections are treated as immutable). If collections are mutable, concurrent access requires external synchronization.
- Empty collections: `Arguments` and `Fields` may be empty lists, but never null. Constructors or setters should ensure non-null collections.
- Alias precedence: If both `Name` and `Alias` are set, the alias takes precedence in the generated query string.
- Type condition usage: `TypeCondition` is typically used with fragments or interfaces; omitting it implies no type constraint.
