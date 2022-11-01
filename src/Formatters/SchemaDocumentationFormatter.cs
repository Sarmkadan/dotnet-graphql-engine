// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Domain.Entities;
using System.Text;

namespace GraphQLEngine.Formatters;

/// <summary>
/// Formats GraphQL schema as human-readable documentation
/// Generates markdown, HTML, or text documentation from schema definitions
/// </summary>
public class SchemaDocumentationFormatter
{
    private readonly DocumentationFormatterOptions _options;

    public SchemaDocumentationFormatter(DocumentationFormatterOptions? options = null)
    {
        _options = options ?? DocumentationFormatterOptions.Default();
    }

    /// <summary>
    /// Generates Markdown documentation for a schema
    /// </summary>
    public string GenerateMarkdown(GraphQLSchema schema, List<GraphQLType>? types = null)
    {
        var sb = new StringBuilder();

        // Title and description
        sb.AppendLine($"# {schema.Name} API Documentation");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(schema.Description))
        {
            sb.AppendLine($"**Description:** {schema.Description}");
            sb.AppendLine();
        }

        // Table of contents
        sb.AppendLine("## Table of Contents");
        sb.AppendLine("- [Query](#query)");
        sb.AppendLine("- [Mutation](#mutation)");
        if (schema.SubscriptionType != null)
            sb.AppendLine("- [Subscription](#subscription)");
        sb.AppendLine("- [Types](#types)");
        sb.AppendLine();

        // Query type
        if (schema.QueryType != null)
        {
            sb.AppendLine("## Query");
            sb.AppendLine(GenerateTypeMarkdown(schema.QueryType, 3));
        }

        // Mutation type
        if (schema.MutationType != null)
        {
            sb.AppendLine("## Mutation");
            sb.AppendLine(GenerateTypeMarkdown(schema.MutationType, 3));
        }

        // Subscription type
        if (schema.SubscriptionType != null)
        {
            sb.AppendLine("## Subscription");
            sb.AppendLine(GenerateTypeMarkdown(schema.SubscriptionType, 3));
        }

        // Types
        if (types != null && types.Count > 0)
        {
            sb.AppendLine("## Types");
            sb.AppendLine();

            foreach (var type in types)
            {
                sb.AppendLine(GenerateTypeMarkdown(type, 3));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates HTML documentation for a schema
    /// </summary>
    public string GenerateHTML(GraphQLSchema schema)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine($"<title>{schema.Name} API Documentation</title>");
        sb.AppendLine("<style>");
        sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
        sb.AppendLine("h1 { color: #333; }");
        sb.AppendLine("code { background-color: #f4f4f4; padding: 2px 6px; }");
        sb.AppendLine(".type { border-left: 4px solid #0066cc; padding-left: 12px; margin: 12px 0; }");
        sb.AppendLine("</style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h1>{schema.Name}</h1>");
        sb.AppendLine($"<p>{schema.Description}</p>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Generates plain text documentation
    /// </summary>
    public string GenerateText(GraphQLSchema schema)
    {
        var sb = new StringBuilder();

        sb.AppendLine(new string('=', 50));
        sb.AppendLine($"Schema: {schema.Name}");
        sb.AppendLine(new string('=', 50));
        sb.AppendLine();

        if (!string.IsNullOrEmpty(schema.Description))
        {
            sb.AppendLine($"Description: {schema.Description}");
            sb.AppendLine();
        }

        if (schema.QueryType != null)
        {
            sb.AppendLine("QUERY ROOT TYPE:");
            sb.AppendLine(GenerateTypeText(schema.QueryType, indentLevel: 1));
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates a quick reference guide
    /// </summary>
    public string GenerateQuickReference(GraphQLSchema schema)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"## Quick Reference - {schema.Name}");
        sb.AppendLine();

        if (schema.QueryType?.Fields != null)
        {
            sb.AppendLine("### Queries:");
            foreach (var field in schema.QueryType.Fields)
            {
                sb.AppendLine($"  - `{field.Name}`: {field.Description}");
            }
            sb.AppendLine();
        }

        if (schema.MutationType?.Fields != null)
        {
            sb.AppendLine("### Mutations:");
            foreach (var field in schema.MutationType.Fields)
            {
                sb.AppendLine($"  - `{field.Name}`: {field.Description}");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates example queries for a schema
    /// </summary>
    public string GenerateExamples(GraphQLSchema schema)
    {
        var sb = new StringBuilder();

        sb.AppendLine("## Example Queries");
        sb.AppendLine();

        if (schema.QueryType?.Fields != null && schema.QueryType.Fields.Any())
        {
            sb.AppendLine("### Basic Queries");
            sb.AppendLine("```graphql");

            var firstField = schema.QueryType.Fields.First();
            sb.AppendLine($"query {{");
            sb.AppendLine($"  {firstField.Name} {{");
            sb.AppendLine($"    # Add fields here");
            sb.AppendLine($"  }}");
            sb.AppendLine($"}}");

            sb.AppendLine("```");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates type documentation in Markdown format
    /// </summary>
    private string GenerateTypeMarkdown(GraphQLType type, int headingLevel)
    {
        var sb = new StringBuilder();
        var heading = new string('#', headingLevel);

        sb.AppendLine($"{heading} {type.Name}");
        sb.AppendLine();

        if (!string.IsNullOrEmpty(type.Description))
        {
            sb.AppendLine($"**Description:** {type.Description}");
            sb.AppendLine();
        }

        sb.AppendLine($"**Kind:** `{type.Kind}`");
        sb.AppendLine();

        if (type.Fields != null && type.Fields.Count > 0)
        {
            sb.AppendLine($"{heading}# Fields");
            sb.AppendLine();

            foreach (var field in type.Fields)
            {
                sb.AppendLine($"- **{field.Name}**: `{field.ReturnType}`");
                if (!string.IsNullOrEmpty(field.Description))
                    sb.AppendLine($"  - {field.Description}");
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates type documentation in text format
    /// </summary>
    private string GenerateTypeText(GraphQLType type, int indentLevel = 0)
    {
        var indent = new string(' ', indentLevel * 2);
        var sb = new StringBuilder();

        sb.AppendLine($"{indent}Type: {type.Name}");
        sb.AppendLine($"{indent}Kind: {type.Kind}");

        if (type.Fields != null)
        {
            sb.AppendLine($"{indent}Fields:");
            foreach (var field in type.Fields)
            {
                sb.AppendLine($"{indent}  - {field.Name}: {field.ReturnType}");
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// Options for documentation formatting
/// </summary>
public class DocumentationFormatterOptions
{
    public bool IncludeExamples { get; set; } = true;
    public bool IncludeDeprecated { get; set; } = true;
    public bool IncludeInternalFields { get; set; } = false;
    public string Language { get; set; } = "en";
    public int MaxDepth { get; set; } = 5;

    public static DocumentationFormatterOptions Default()
    {
        return new DocumentationFormatterOptions();
    }
}
