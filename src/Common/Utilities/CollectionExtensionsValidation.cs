#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Globalization;

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Validation utilities for <see cref="CollectionExtensions"/> static class.
/// </summary>
public static class CollectionExtensionsValidation
{
    /// <summary>
    /// Validates various methods of <see cref="CollectionExtensions"/> and returns human-readable problems.
    /// Note: This is implemented as a static method without parameters to avoid CS0721 (static types cannot be used as parameters).
    /// </summary>
    /// <returns>List of validation problems, empty if valid.</returns>
    public static IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        // Validate IsNullOrEmpty
        var listNull = (List<string>?)null;
        if (!CollectionExtensions.IsNullOrEmpty(listNull))
        {
            errors.Add("IsNullOrEmpty failed for null");
        }
        
        var listEmpty = new List<string>();
        if (!CollectionExtensions.IsNullOrEmpty(listEmpty))
        {
            errors.Add("IsNullOrEmpty failed for empty list");
        }

        var listNotEmpty = new List<string> { "test" };
        if (CollectionExtensions.IsNullOrEmpty(listNotEmpty))
        {
            errors.Add("IsNullOrEmpty failed for non-empty list");
        }

        // Validate HasItems
        if (CollectionExtensions.HasItems(listNull))
        {
            errors.Add("HasItems failed for null");
        }

        if (CollectionExtensions.HasItems(listEmpty))
        {
            errors.Add("HasItems failed for empty list");
        }

        if (!CollectionExtensions.HasItems(listNotEmpty))
        {
            errors.Add("HasItems failed for non-empty list");
        }

        // Validate FirstOrNull
        if (CollectionExtensions.FirstOrNull(listEmpty) != null)
        {
            errors.Add("FirstOrNull failed for empty list");
        }

        if (CollectionExtensions.FirstOrNull(listNotEmpty) != "test")
        {
            errors.Add("FirstOrNull failed for non-empty list");
        }

        // Validate AddIfNotNull
        var listToAdd = new List<string>();
        CollectionExtensions.AddIfNotNull(listToAdd, "test");
        if (listToAdd.Count != 1 || listToAdd[0] != "test")
        {
            errors.Add("AddIfNotNull failed");
        }
        
        CollectionExtensions.AddIfNotNull(listToAdd, (string?)null);
        if (listToAdd.Count != 1)
        {
            errors.Add("AddIfNotNull failed to skip null");
        }

        // Validate AddRange
        var listRange = new List<string>();
        CollectionExtensions.AddRange(listRange, new[] { "a", "b" });
        if (listRange.Count != 2 || listRange[0] != "a" || listRange[1] != "b")
        {
            errors.Add("AddRange failed");
        }

        // Validate RemoveRange
        CollectionExtensions.RemoveRange(listRange, new[] { "a" });
        if (listRange.Count != 1 || listRange[0] != "b")
        {
            errors.Add("RemoveRange failed");
        }

        // Validate Batch
        var batchResult = CollectionExtensions.Batch(new[] { 1, 2, 3, 4 }, 2).ToList();
        if (batchResult.Count != 2 || batchResult[0].Count != 2 || batchResult[1].Count != 2)
        {
            errors.Add("Batch failed");
        }

        // Validate DistinctBy
        var distinctResult = CollectionExtensions.DistinctBy(new[] { "a", "a", "b" }, x => x).ToList();
        if (distinctResult.Count != 2 || distinctResult[0] != "a" || distinctResult[1] != "b")
        {
            errors.Add("DistinctBy failed");
        }

        // Validate IndexOf
        if (CollectionExtensions.IndexOf(new[] { "a", "b" }, "b") != 1)
        {
            errors.Add("IndexOf failed");
        }

        // Validate ForEach
        int forEachCount = 0;
        CollectionExtensions.ForEach(new[] { 1, 2 }, x => forEachCount += x);
        if (forEachCount != 3)
        {
            errors.Add("ForEach failed");
        }

        // Validate ForEachWithIndex
        int forEachIndexCount = 0;
        CollectionExtensions.ForEachWithIndex(new[] { 1, 2 }, (x, i) => forEachIndexCount += i);
        if (forEachIndexCount != 1)
        {
            errors.Add("ForEachWithIndex failed");
        }

        // Validate All
        if (!CollectionExtensions.All(new[] { "a", "a" }, "a"))
        {
            errors.Add("All failed");
        }

        // Validate Combine
        var combined = CollectionExtensions.Combine(new[] { 1 }, new[] { 2 }).ToList();
        if (combined.Count != 2 || combined[0] != 1 || combined[1] != 2)
        {
            errors.Add("Combine failed");
        }

        // Validate Random
        var randomItem = CollectionExtensions.Random(new[] { 1 });
        if (randomItem != 1)
        {
            errors.Add("Random failed");
        }

        // Validate Shuffle
        var shuffled = CollectionExtensions.Shuffle(new[] { 1, 2, 3 }).ToList();
        if (shuffled.Count != 3)
        {
            errors.Add("Shuffle failed");
        }

        // Validate CountBy
        var countBy = CollectionExtensions.CountBy(new[] { "a", "b", "a" }, x => x);
        if (countBy.Count != 2 || countBy["a"] != 2 || countBy["b"] != 1)
        {
            errors.Add("CountBy failed");
        }

        // Validate ToDictionary
        var dict = CollectionExtensions.ToDictionary(new[] { "a", "b" }, x => x, 1);
        if (dict.Count != 2 || dict["a"] != 1 || dict["b"] != 1)
        {
            errors.Add("ToDictionary failed");
        }

        // Validate OrderByMany
        var ordered = CollectionExtensions.OrderByMany(new[] { 2, 1 }, x => x).ToList();
        if (ordered.Count != 2 || ordered[0] != 1 || ordered[1] != 2)
        {
            errors.Add("OrderByMany failed");
        }

        // Validate Flatten
        var flattened = CollectionExtensions.Flatten(new[] { new[] { 1 }, new[] { 2 } }).ToList();
        if (flattened.Count != 2 || flattened[0] != 1 || flattened[1] != 2)
        {
            errors.Add("Flatten failed");
        }

        // Validate Median
        var median = CollectionExtensions.Median(new[] { 1, 3, 2 });
        if (median != 2)
        {
            errors.Add("Median failed");
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Checks if the CollectionExtensions is valid (all validation methods pass).
    /// </summary>
    /// <returns>True if valid, false otherwise.</returns>
    public static bool IsValid()
    {
        return Validate().Count == 0;
    }

    /// <summary>
    /// Ensures the CollectionExtensions is valid, throwing ArgumentException if not.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when validation fails with list of problems.</exception>
    public static void EnsureValid()
    {
        var errors = Validate();

        if (errors.Count > 0)
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine("CollectionExtensions validation failed:");

            for (int i = 0; i < errors.Count; i++)
            {
                errorMessage.AppendLine($" {i + 1}. {errors[i]}");
            }

            throw new ArgumentException(errorMessage.ToString());
        }
    }
}
