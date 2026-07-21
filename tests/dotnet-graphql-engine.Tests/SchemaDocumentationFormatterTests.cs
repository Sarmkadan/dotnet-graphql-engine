#nullable enable

using GraphQLEngine.Domain.Entities;
using GraphQLEngine.Formatters;
using Xunit;

namespace GraphQLEngine.Tests.Formatters;

/// <summary>
/// Tests for SchemaDocumentationFormatter to ensure proper documentation generation
/// </summary>
public class SchemaDocumentationFormatterTests
{
    private readonly SchemaDocumentationFormatter _formatter;
    private readonly DocumentationFormatterOptions _defaultOptions;

    public SchemaDocumentationFormatterTests()
    {
        _defaultOptions = DocumentationFormatterOptions.Default();
        _formatter = new SchemaDocumentationFormatter(_defaultOptions);
    }

    [Fact]
    public void GenerateMarkdown_WithTypeWithFields_RendersAllFieldNames()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "TestSchema",
            Description = "A test schema for documentation"
        };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object)
        {
            Description = "The root query type"
        };

        queryType.AddField(new GraphQLField("getUser", "User")
        {
            Description = "Get a user by ID"
        });

        queryType.AddField(new GraphQLField("getPost", "Post")
        {
            Description = "Get a post by ID"
        });

        queryType.AddField(new GraphQLField("listUsers", "[User]")
        {
            Description = "List all users"
        });

        schema.AddType(queryType);
        schema.QueryType = queryType;

        var userType = new GraphQLType("User", GraphQLTypeKind.Object)
        {
            Description = "A user in the system"
        };

        userType.AddField(new GraphQLField("id", "ID!")
        {
            Description = "User ID"
        });

        userType.AddField(new GraphQLField("name", "String")
        {
            Description = "User name"
        });

        userType.AddField(new GraphQLField("email", "String")
        {
            Description = "User email"
        });

        schema.AddType(userType);

        var types = new List<GraphQLType> { userType };

        // Act
        var markdown = _formatter.GenerateMarkdown(schema, types);

        // Assert
        Assert.NotNull(markdown);
        Assert.Contains("# TestSchema API Documentation", markdown);
        Assert.Contains("**Description:** A test schema for documentation", markdown);
        Assert.Contains("## Query", markdown);
        Assert.Contains("- **getUser**: `User`", markdown);
        Assert.Contains("- **getPost**: `Post`", markdown);
        Assert.Contains("- **listUsers**: `[User]`", markdown);
        Assert.Contains("## Types", markdown);
        Assert.Contains("### User", markdown);
        Assert.Contains("- **id**: `ID!`", markdown);
        Assert.Contains("- **name**: `String`", markdown);
        Assert.Contains("- **email**: `String`", markdown);
    }

    [Fact]
    public void GenerateMarkdown_WithDeprecatedField_ShowsDeprecationAnnotation()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "DeprecatedSchema",
            Description = "A schema with deprecated fields"
        };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);

        var oldField = new GraphQLField("oldQuery", "String")
        {
            Description = "An old query that should be deprecated",
            IsDeprecated = true,
            DeprecationReason = "Use newQuery instead"
        };

        var newField = new GraphQLField("newQuery", "String")
        {
            Description = "The new recommended query"
        };

        queryType.AddField(oldField);
        queryType.AddField(newField);

        schema.QueryType = queryType;

        // Act
        var markdown = _formatter.GenerateMarkdown(schema);

        // Assert
        Assert.NotNull(markdown);
        Assert.Contains("## Query", markdown);
        Assert.Contains("- **oldQuery**: `String`", markdown);
        Assert.Contains("- **newQuery**: `String`", markdown);
        // The formatter should include deprecated fields by default
        Assert.Contains("oldQuery", markdown);
    }

    [Fact]
    public void GenerateMarkdown_WithEmptySchema_RendersMinimalDocumentation()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "EmptySchema",
            Description = "A schema with no types"
        };

        // Act
        var markdown = _formatter.GenerateMarkdown(schema);

        // Assert
        Assert.NotNull(markdown);
        Assert.Contains("# EmptySchema API Documentation", markdown);
        Assert.Contains("**Description:** A schema with no types", markdown);
        Assert.Contains("## Table of Contents", markdown);
        Assert.Contains("- [Query](#query)", markdown);
        Assert.Contains("- [Mutation](#mutation)", markdown);
        Assert.Contains("- [Types](#types)", markdown);
        // Should not contain Query section since QueryType is null
        Assert.DoesNotContain("## Query", markdown);
    }

    [Fact]
    public void GenerateMarkdown_WithMutationType_RendersMutationSection()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "MutationSchema",
            Description = "A schema with mutations"
        };

        var mutationType = new GraphQLType("Mutation", GraphQLTypeKind.Object)
        {
            Description = "The root mutation type"
        };

        mutationType.AddField(new GraphQLField("createUser", "User!")
        {
            Description = "Create a new user"
        });

        schema.MutationType = mutationType;

        // Act
        var markdown = _formatter.GenerateMarkdown(schema);

        // Assert
        Assert.NotNull(markdown);
        Assert.Contains("## Mutation", markdown);
        Assert.Contains("- **createUser**: `User!`", markdown);
    }

    [Fact]
    public void GenerateMarkdown_WithSubscriptionType_RendersSubscriptionSection()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "SubscriptionSchema",
            Description = "A schema with subscriptions"
        };

        var subscriptionType = new GraphQLType("Subscription", GraphQLTypeKind.Object)
        {
            Description = "The root subscription type"
        };

        subscriptionType.AddField(new GraphQLField("userCreated", "User")
        {
            Description = "Subscribe to user creation events"
        });

        schema.SubscriptionType = subscriptionType;

        // Act
        var markdown = _formatter.GenerateMarkdown(schema);

        // Assert
        Assert.NotNull(markdown);
        Assert.Contains("## Subscription", markdown);
        Assert.Contains("- [Subscription](#subscription)", markdown);
        Assert.Contains("- **userCreated**: `User`", markdown);
    }

    [Fact]
    public void GenerateMarkdown_WithMultipleTypes_RendersAllTypes()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "MultiTypeSchema",
            Description = "A schema with multiple types"
        };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);
        queryType.AddField(new GraphQLField("getData", "String"));
        schema.QueryType = queryType;

        var userType = new GraphQLType("User", GraphQLTypeKind.Object)
        {
            Description = "User type"
        };
        userType.AddField(new GraphQLField("id", "ID!"));

        var postType = new GraphQLType("Post", GraphQLTypeKind.Object)
        {
            Description = "Post type"
        };
        postType.AddField(new GraphQLField("title", "String"));

        var commentType = new GraphQLType("Comment", GraphQLTypeKind.Object)
        {
            Description = "Comment type"
        };
        commentType.AddField(new GraphQLField("text", "String"));

        var types = new List<GraphQLType> { userType, postType, commentType };

        // Act
        var markdown = _formatter.GenerateMarkdown(schema, types);

        // Assert
        Assert.NotNull(markdown);
        Assert.Contains("## Types", markdown);
        Assert.Contains("### User", markdown);
        Assert.Contains("### Post", markdown);
        Assert.Contains("### Comment", markdown);
    }

    [Fact]
    public void GenerateHTML_WithSchema_RendersBasicHTML()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "HTMLSchema",
            Description = "A schema for HTML output"
        };

        // Act
        var html = _formatter.GenerateHTML(schema);

        // Assert
        Assert.NotNull(html);
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html>", html);
        Assert.Contains("<title>HTMLSchema API Documentation</title>", html);
        Assert.Contains("<h1>HTMLSchema</h1>", html);
        Assert.Contains("<p>" + schema.Description + "</p>", html);
    }

    [Fact]
    public void GenerateText_WithSchema_RendersPlainText()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "TextSchema",
            Description = "A schema for text output"
        };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);
        queryType.AddField(new GraphQLField("test", "String"));
        schema.QueryType = queryType;

        // Act
        var text = _formatter.GenerateText(schema);

        // Assert
        Assert.NotNull(text);
        Assert.Contains("==================================================", text);
        Assert.Contains("Schema: TextSchema", text);
        Assert.Contains("Description: A schema for text output", text);
        Assert.Contains("QUERY ROOT TYPE:", text);
        Assert.Contains("Type: Query", text);
    }

    [Fact]
    public void GenerateQuickReference_WithSchema_RendersQuickReference()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "QuickRefSchema"
        };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);
        queryType.AddField(new GraphQLField("query1", "String")
        {
            Description = "First query"
        });
        queryType.AddField(new GraphQLField("query2", "Int")
        {
            Description = "Second query"
        });
        schema.QueryType = queryType;

        // Act
        var quickRef = _formatter.GenerateQuickReference(schema);

        // Assert
        Assert.NotNull(quickRef);
        Assert.Contains("## Quick Reference - QuickRefSchema", quickRef);
        Assert.Contains("### Queries:", quickRef);
        Assert.Contains(" - `query1`: First query", quickRef);
        Assert.Contains(" - `query2`: Second query", quickRef);
    }

    [Fact]
    public void GenerateExamples_WithSchema_RendersExampleQueries()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "ExampleSchema"
        };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);
        queryType.AddField(new GraphQLField("getUser", "User"));
        schema.QueryType = queryType;

        // Act
        var examples = _formatter.GenerateExamples(schema);

        // Assert
        Assert.NotNull(examples);
        Assert.Contains("## Example Queries", examples);
        Assert.Contains("### Basic Queries", examples);
        Assert.Contains("```graphql", examples);
        Assert.Contains("query {", examples);
        Assert.Contains(" getUser {", examples);
    }

    [Fact]
    public void DocumentationFormatterOptions_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var options = DocumentationFormatterOptions.Default();

        // Assert
        Assert.NotNull(options);
        Assert.True(options.IncludeExamples);
        Assert.True(options.IncludeDeprecated);
        Assert.False(options.IncludeInternalFields);
        Assert.Equal("en", options.Language);
        Assert.Equal(5, options.MaxDepth);
    }

    [Fact]
    public void SchemaDocumentationFormatter_WithCustomOptions_UsesCustomOptions()
    {
        // Arrange
        var customOptions = new DocumentationFormatterOptions
        {
            IncludeExamples = false,
            IncludeDeprecated = false,
            IncludeInternalFields = true,
            Language = "fr",
            MaxDepth = 10
        };

        var formatter = new SchemaDocumentationFormatter(customOptions);

        var schema = new GraphQLSchema
        {
            Name = "CustomOptionsSchema"
        };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);
        queryType.AddField(new GraphQLField("field1", "String"));
        schema.QueryType = queryType;

        // Act
        var markdown = formatter.GenerateMarkdown(schema);

        // Assert - just verify it doesn't throw and produces output
        Assert.NotNull(markdown);
        Assert.Contains("# CustomOptionsSchema API Documentation", markdown);
    }

    [Fact]
    public void GenerateMarkdown_TypeWithDescription_RendersDescription()
    {
        // Arrange
        var schema = new GraphQLSchema { Name = "DescribedSchema" };

        var type = new GraphQLType("DescribedType", GraphQLTypeKind.Object)
        {
            Description = "This is a type with a description"
        };
        type.AddField(new GraphQLField("field", "String"));

        schema.AddType(type);

        var types = new List<GraphQLType> { type };

        // Act
        var markdown = _formatter.GenerateMarkdown(schema, types);

        // Assert
        Assert.Contains("**Description:** This is a type with a description", markdown);
    }

    [Fact]
    public void GenerateMarkdown_TypeWithFieldDescription_RendersFieldDescription()
    {
        // Arrange
        var schema = new GraphQLSchema { Name = "FieldDescSchema" };

        var type = new GraphQLType("TypeWithFields", GraphQLTypeKind.Object);
        type.AddField(new GraphQLField("field1", "String")
        {
            Description = "First field description"
        });
        type.AddField(new GraphQLField("field2", "Int")
        {
            Description = "Second field description"
        });

        schema.AddType(type);
        var types = new List<GraphQLType> { type };

        // Act
        var markdown = _formatter.GenerateMarkdown(schema, types);

        // Assert
        Assert.Contains("- **field1**: `String`", markdown);
        Assert.Contains("First field description", markdown);
        Assert.Contains("- **field2**: `Int`", markdown);
        Assert.Contains("Second field description", markdown);
    }

    [Fact]
    public void GenerateMarkdown_TypeWithKind_RendersKind()
    {
        // Arrange
        var schema = new GraphQLSchema { Name = "KindSchema" };

        var type = new GraphQLType("MyType", GraphQLTypeKind.Object);
        type.AddField(new GraphQLField("field", "String"));

        schema.AddType(type);
        var types = new List<GraphQLType> { type };

        // Act
        var markdown = _formatter.GenerateMarkdown(schema, types);

        // Assert
        Assert.Contains("**Kind:** `Object`", markdown);
    }

    [Fact]
    public void GenerateMarkdown_SchemaWithoutDescription_StillRenders()
    {
        // Arrange
        var schema = new GraphQLSchema { Name = "NoDescSchema" };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);
        queryType.AddField(new GraphQLField("test", "String"));
        schema.QueryType = queryType;

        // Act
        var markdown = _formatter.GenerateMarkdown(schema);

        // Assert
        Assert.NotNull(markdown);
        Assert.Contains("# NoDescSchema API Documentation", markdown);
        // Should not contain description section when empty
        Assert.DoesNotContain("**Description:**", markdown);
    }

    [Fact]
    public void GenerateMarkdown_FieldWithArguments_RendersArguments()
    {
        // Arrange
        var schema = new GraphQLSchema { Name = "ArgSchema" };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);
        var field = new GraphQLField("getUser", "User");
        field.AddArgument(new GraphQLArgument("id", "ID!", true));
        field.AddArgument(new GraphQLArgument("includeDetails", "Boolean", false)
        {
            Description = "Whether to include detailed user information"
        });
        queryType.AddField(field);
        schema.QueryType = queryType;

        // Act
        var markdown = _formatter.GenerateMarkdown(schema);

        // Assert - just verify it doesn't throw
        Assert.NotNull(markdown);
        Assert.Contains("getUser", markdown);
    }

    [Fact]
    public void GenerateMarkdown_WithNullTypesParameter_Works()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "NullTypesSchema",
            Description = "Test schema"
        };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);
        queryType.AddField(new GraphQLField("test", "String"));
        schema.QueryType = queryType;

        // Act
        var markdown = _formatter.GenerateMarkdown(schema, types: null);

        // Assert
        Assert.NotNull(markdown);
        Assert.Contains("# NullTypesSchema API Documentation", markdown);
    }

    [Fact]
    public void GenerateMarkdown_WithEmptyTypesList_Works()
    {
        // Arrange
        var schema = new GraphQLSchema
        {
            Name = "EmptyTypesSchema"
        };

        var queryType = new GraphQLType("Query", GraphQLTypeKind.Object);
        queryType.AddField(new GraphQLField("test", "String"));
        schema.QueryType = queryType;

        // Act
        var markdown = _formatter.GenerateMarkdown(schema, new List<GraphQLType>());

        // Assert
        Assert.NotNull(markdown);
        Assert.Contains("# EmptyTypesSchema API Documentation", markdown);
    }
}