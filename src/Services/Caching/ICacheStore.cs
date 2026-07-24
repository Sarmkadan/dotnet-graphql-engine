#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Services.Caching;

/// <summary>
/// Bounded key/value cache abstraction used by the engine for the persisted-query
/// index and the parsed-document cache. Implementations must be thread-safe and
/// must enforce an upper bound on the number of entries so that attacker-controlled
/// keys (e.g. unlimited unique persisted-query hashes) cannot exhaust memory.
/// </summary>
/// <typeparam name="TKey">Cache key type.</typeparam>
/// <typeparam name="TValue">Cached value type.</typeparam>
public interface ICacheStore<TKey, TValue> where TKey : notnull
{
    /// <summary>Gets the current number of live entries in the store.</summary>
    int Count { get; }

    /// <summary>Gets the maximum number of entries the store will retain.</summary>
    int MaxEntries { get; }

    /// <summary>
    /// Attempts to retrieve the value associated with <paramref name="key"/>.
    /// Expired entries are treated as absent.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="value">The cached value when found; default otherwise.</param>
    /// <returns><c>true</c> when a live entry exists; <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <c>null</c>.</exception>
    bool TryGet(TKey key, out TValue value);

    /// <summary>
    /// Adds or replaces the entry for <paramref name="key"/>, evicting the least
    /// recently used entry when the store is at capacity.
    /// </summary>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The value to cache.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <c>null</c>.</exception>
    void Set(TKey key, TValue value);

    /// <summary>
    /// Returns the cached value for <paramref name="key"/>, or computes it via
    /// <paramref name="valueFactory"/>, caches it, and returns it.
    /// </summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="valueFactory">Factory invoked on a cache miss.</param>
    /// <returns>The cached or newly computed value.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="key"/> or <paramref name="valueFactory"/> is <c>null</c>.
    /// </exception>
    TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);

    /// <summary>
    /// Removes the entry associated with <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns><c>true</c> when an entry was removed; <c>false</c> otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <c>null</c>.</exception>
    bool Remove(TKey key);

    /// <summary>Removes all entries from the store.</summary>
    void Clear();

    /// <summary>
    /// Returns a point-in-time snapshot of the values currently held by the store,
    /// excluding expired entries. Intended for statistics/reporting; callers must
    /// not rely on the snapshot reflecting subsequent mutations.
    /// </summary>
    IReadOnlyCollection<TValue> Snapshot();
}
