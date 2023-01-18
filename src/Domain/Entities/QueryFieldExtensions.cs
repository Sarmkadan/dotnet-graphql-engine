using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphQLEngine.Domain.Entities
{
    /// <summary>
    /// Provides extension methods for working with <see cref="QueryField"/> instances.
    /// </summary>
    public static class QueryFieldExtensions
    {
        /// <summary>
        /// Returns the display name of the query field, preferring the alias if present.
        /// </summary>
        /// <param name="field">The query field to get the display name for. Cannot be <see langword="null"/>.</param>
        /// <returns>The display name (alias if present, otherwise the field name).</returns>
        /// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
        public static string GetDisplayName(this QueryField field)
            => field.Alias ?? field.Name ?? throw new ArgumentNullException(nameof(field));

        /// <summary>
        /// Determines whether the query field is a scalar (has no subfields and no type condition).
        /// </summary>
        /// <param name="field">The query field to check. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the field is a scalar; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
        public static bool IsScalar(this QueryField field)
            => field is null
                ? throw new ArgumentNullException(nameof(field))
                : (field.Fields.Count == 0 && string.IsNullOrEmpty(field.TypeCondition));

        /// <summary>
        /// Determines whether the query field has any arguments.
        /// </summary>
        /// <param name="field">The query field to check. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the field has arguments; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="field"/> is <see langword="null"/>.</exception>
        public static bool HasArguments(this QueryField field)
            => field is null
                ? throw new ArgumentNullException(nameof(field))
                : field.Arguments.Count > 0;
    }
}