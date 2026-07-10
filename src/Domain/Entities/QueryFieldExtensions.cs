using System.Collections.Generic;
using System.Linq;

namespace GraphQLEngine.Domain.Entities
{
    public static class QueryFieldExtensions
    {
        /// <summary>
        /// Returns the display name of the query field, preferring the alias if present.
        /// </summary>
        public static string GetDisplayName(this QueryField field)
        {
            return field.Alias ?? field.Name;
        }

        /// <summary>
        /// Determines whether the query field is a scalar (has no subfields and no type condition).
        /// </summary>
        public static bool IsScalar(this QueryField field)
        {
            return field.Fields == null || !field.Fields.Any() && string.IsNullOrEmpty(field.TypeCondition);
        }

        /// <summary>
        /// Determines whether the query field has any arguments.
        /// </summary>
        public static bool HasArguments(this QueryField field)
        {
            return field.Arguments != null && field.Arguments.Any();
        }
    }
}
