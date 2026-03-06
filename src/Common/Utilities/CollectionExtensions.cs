// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Common.Utilities;

/// <summary>
/// Extension methods for collections and enumerables
/// Provides utilities for common collection operations
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Checks if a collection is null or empty
    /// </summary>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection == null || !collection.Any();
    }

    /// <summary>
    /// Checks if a collection has items
    /// </summary>
    public static bool HasItems<T>(this IEnumerable<T>? collection)
    {
        return collection != null && collection.Any();
    }

    /// <summary>
    /// Gets the first item or null instead of throwing exception
    /// </summary>
    public static T? FirstOrNull<T>(this IEnumerable<T>? collection) where T : class
    {
        return collection?.FirstOrDefault();
    }

    /// <summary>
    /// Safely adds an item to a collection
    /// </summary>
    public static void AddIfNotNull<T>(this ICollection<T> collection, T? item)
    {
        if (item != null)
            collection.Add(item);
    }

    /// <summary>
    /// Adds multiple items to a collection
    /// </summary>
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T>? items)
    {
        if (items == null)
            return;

        foreach (var item in items)
            collection.Add(item);
    }

    /// <summary>
    /// Removes multiple items from a collection
    /// </summary>
    public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T>? items)
    {
        if (items == null)
            return;

        foreach (var item in items)
            collection.Remove(item);
    }

    /// <summary>
    /// Splits a collection into batches
    /// </summary>
    public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
    {
        if (batchSize <= 0)
            throw new ArgumentException("Batch size must be greater than 0", nameof(batchSize));

        var batch = new List<T>(batchSize);
        foreach (var item in collection)
        {
            batch.Add(item);
            if (batch.Count >= batchSize)
            {
                yield return batch;
                batch = new List<T>(batchSize);
            }
        }

        if (batch.Count > 0)
            yield return batch;
    }

    /// <summary>
    /// Gets distinct items by a key selector
    /// </summary>
    public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> collection,
        Func<T, TKey> keySelector)
    {
        var seenKeys = new HashSet<TKey>();
        foreach (var item in collection)
        {
            var key = keySelector(item);
            if (seenKeys.Add(key))
                yield return item;
        }
    }

    /// <summary>
    /// Returns the index of an item in a collection
    /// </summary>
    public static int IndexOf<T>(this IEnumerable<T> collection, T item)
    {
        var index = 0;
        foreach (var current in collection)
        {
            if (Equals(current, item))
                return index;
            index++;
        }
        return -1;
    }

    /// <summary>
    /// Executes an action for each item in a collection
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
            action(item);
    }

    /// <summary>
    /// Executes an action for each item with its index
    /// </summary>
    public static void ForEachWithIndex<T>(this IEnumerable<T> collection, Action<T, int> action)
    {
        var index = 0;
        foreach (var item in collection)
        {
            action(item, index);
            index++;
        }
    }

    /// <summary>
    /// Checks if a collection matches all criteria
    /// </summary>
    public static bool All<T>(this IEnumerable<T> collection, T? value)
    {
        return collection.All(item => Equals(item, value));
    }

    /// <summary>
    /// Combines multiple collections
    /// </summary>
    public static IEnumerable<T> Combine<T>(params IEnumerable<T>?[] collections)
    {
        foreach (var collection in collections.Where(c => c != null))
        {
            foreach (var item in collection!)
                yield return item;
        }
    }

    /// <summary>
    /// Gets a random item from a collection
    /// </summary>
    public static T? Random<T>(this IEnumerable<T> collection)
    {
        var list = collection.ToList();
        if (list.Count == 0)
            return default;

        var random = new System.Random();
        return list[random.Next(list.Count)];
    }

    /// <summary>
    /// Shuffles a collection
    /// </summary>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> collection)
    {
        var list = collection.ToList();
        var random = new System.Random();

        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = random.Next(0, i + 1);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }

        return list;
    }

    /// <summary>
    /// Groups and counts items
    /// </summary>
    public static Dictionary<TKey, int> CountBy<T, TKey>(this IEnumerable<T> collection,
        Func<T, TKey> keySelector) where TKey : notnull
    {
        var result = new Dictionary<TKey, int>();
        foreach (var item in collection)
        {
            var key = keySelector(item);
            if (result.ContainsKey(key))
                result[key]++;
            else
                result[key] = 1;
        }
        return result;
    }

    /// <summary>
    /// Converts a collection to a dictionary with a default value
    /// </summary>
    public static Dictionary<TKey, TValue> ToDictionary<T, TKey, TValue>(
        this IEnumerable<T> collection,
        Func<T, TKey> keySelector,
        TValue defaultValue) where TKey : notnull
    {
        return collection.ToDictionary(keySelector, _ => defaultValue);
    }

    /// <summary>
    /// Sorts a collection in a specified order
    /// </summary>
    public static IEnumerable<T> OrderByMany<T>(this IEnumerable<T> collection,
        params Func<T, IComparable>[] keySelectors)
    {
        var enumerable = collection;
        for (int i = keySelectors.Length - 1; i >= 0; i--)
        {
            enumerable = enumerable.OrderBy(keySelectors[i]);
        }
        return enumerable;
    }

    /// <summary>
    /// Flattens nested collections
    /// </summary>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> collection)
    {
        return collection.SelectMany(x => x);
    }

    /// <summary>
    /// Gets the median value
    /// </summary>
    public static T? Median<T>(this IEnumerable<T> collection) where T : IComparable<T>
    {
        var sorted = collection.OrderBy(x => x).ToList();
        if (sorted.Count == 0)
            return default;

        var middle = sorted.Count / 2;
        return sorted.Count % 2 == 1
            ? sorted[middle]
            : sorted[middle - 1];
    }
}
