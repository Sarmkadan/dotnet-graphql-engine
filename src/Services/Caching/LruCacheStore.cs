#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Services.Caching;

/// <summary>
/// Default <see cref="ICacheStore{TKey, TValue}"/> implementation with strict
/// least-recently-used eviction and an optional per-entry time-to-live.
/// All operations are O(1) (dictionary + intrusive linked list) and thread-safe
/// via a single lock, which is appropriate for the small, hot caches the engine
/// uses (persisted-query hash index, parsed-document cache).
/// </summary>
/// <typeparam name="TKey">Cache key type.</typeparam>
/// <typeparam name="TValue">Cached value type.</typeparam>
public sealed class LruCacheStore<TKey, TValue> : ICacheStore<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, LinkedListNode<Entry>> _map;
    private readonly LinkedList<Entry> _lruOrder = new();
    private readonly object _gate = new();
    private readonly TimeSpan? _timeToLive;

    /// <summary>
    /// Initialises the store with a hard entry limit and an optional TTL.
    /// </summary>
    /// <param name="maxEntries">Maximum number of entries retained; must be positive.</param>
    /// <param name="timeToLive">
    /// Optional lifetime for each entry, measured from the moment it was written.
    /// Pass <c>null</c> for no expiry. Must be positive when provided.
    /// </param>
    /// <param name="keyComparer">Optional equality comparer for keys.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxEntries"/> is not positive, or when
    /// <paramref name="timeToLive"/> is provided but not positive.
    /// </exception>
    public LruCacheStore(int maxEntries, TimeSpan? timeToLive = null, IEqualityComparer<TKey>? keyComparer = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxEntries);

        if (timeToLive is { } ttl && ttl <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(timeToLive), ttl, "Time-to-live must be positive when provided.");

        MaxEntries = maxEntries;
        _timeToLive = timeToLive;
        _map = new Dictionary<TKey, LinkedListNode<Entry>>(keyComparer);
    }

    /// <inheritdoc />
    public int MaxEntries { get; }

    /// <inheritdoc />
    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _map.Count;
            }
        }
    }

    /// <inheritdoc />
    public bool TryGet(TKey key, out TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        lock (_gate)
        {
            if (_map.TryGetValue(key, out var node))
            {
                if (IsExpired(node.Value))
                {
                    RemoveNode(node);
                }
                else
                {
                    Touch(node);
                    value = node.Value.Value;
                    return true;
                }
            }
        }

        value = default!;
        return false;
    }

    /// <inheritdoc />
    public void Set(TKey key, TValue value)
    {
        ArgumentNullException.ThrowIfNull(key);

        lock (_gate)
        {
            if (_map.TryGetValue(key, out var existing))
            {
                existing.Value = new Entry(key, value, DateTime.UtcNow);
                Touch(existing);
                return;
            }

            EvictIfFull();

            var node = _lruOrder.AddFirst(new Entry(key, value, DateTime.UtcNow));
            _map[key] = node;
        }
    }

    /// <inheritdoc />
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);

        if (TryGet(key, out var cached))
            return cached;

        // Compute outside the lock so a slow factory does not block other readers;
        // a racing writer for the same key simply wins with an equivalent value.
        var created = valueFactory(key);
        Set(key, created);
        return created;
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        lock (_gate)
        {
            if (!_map.TryGetValue(key, out var node))
                return false;

            RemoveNode(node);
            return true;
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        lock (_gate)
        {
            _map.Clear();
            _lruOrder.Clear();
        }
    }

    /// <summary>
    /// Removes every expired entry and returns the number of entries evicted.
    /// A no-op returning zero when the store has no TTL configured.
    /// </summary>
    public int RemoveExpired()
    {
        if (_timeToLive is null)
            return 0;

        lock (_gate)
        {
            var removed = 0;
            var node = _lruOrder.Last;

            while (node is not null)
            {
                var previous = node.Previous;
                if (IsExpired(node.Value))
                {
                    RemoveNode(node);
                    removed++;
                }
                node = previous;
            }

            return removed;
        }
    }

    private bool IsExpired(in Entry entry) =>
        _timeToLive is { } ttl && DateTime.UtcNow - entry.WrittenAtUtc > ttl;

    private void Touch(LinkedListNode<Entry> node)
    {
        _lruOrder.Remove(node);
        _lruOrder.AddFirst(node);
    }

    private void RemoveNode(LinkedListNode<Entry> node)
    {
        _map.Remove(node.Value.Key);
        _lruOrder.Remove(node);
    }

    private void EvictIfFull()
    {
        while (_map.Count >= MaxEntries && _lruOrder.Last is { } oldest)
            RemoveNode(oldest);
    }

    private readonly record struct Entry(TKey Key, TValue Value, DateTime WrittenAtUtc);
}
