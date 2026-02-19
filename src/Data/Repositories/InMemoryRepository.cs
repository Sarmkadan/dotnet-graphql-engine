// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace GraphQLEngine.Data.Repositories;

/// <summary>
/// In-memory repository implementation for development and testing
/// </summary>
/// <typeparam name="T">Entity type with Id property</typeparam>
public class InMemoryRepository<T> : IRepository<T> where T : class
{
    private readonly Dictionary<string, T> _store = new();
    private readonly object _lockObject = new();

    public async Task<T?> GetByIdAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        lock (_lockObject)
        {
            _store.TryGetValue(id, out var entity);
            return entity;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        lock (_lockObject)
        {
            return _store.Values.ToList();
        }
    }

    public async Task<T> AddAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        // Extract ID from entity (assumes entity has Id property via reflection)
        var idProp = entity.GetType().GetProperty("Id");
        if (idProp == null)
            throw new InvalidOperationException("Entity must have an Id property");

        var id = idProp.GetValue(entity)?.ToString();
        if (string.IsNullOrEmpty(id))
            throw new InvalidOperationException("Entity Id cannot be empty");

        lock (_lockObject)
        {
            if (_store.ContainsKey(id))
                throw new InvalidOperationException($"Entity with ID {id} already exists");

            _store[id] = entity;
            return entity;
        }
    }

    public async Task<T> UpdateAsync(T entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var idProp = entity.GetType().GetProperty("Id");
        if (idProp == null)
            throw new InvalidOperationException("Entity must have an Id property");

        var id = idProp.GetValue(entity)?.ToString();
        if (string.IsNullOrEmpty(id))
            throw new InvalidOperationException("Entity Id cannot be empty");

        lock (_lockObject)
        {
            if (!_store.ContainsKey(id))
                throw new InvalidOperationException($"Entity with ID {id} not found");

            _store[id] = entity;
            return entity;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        lock (_lockObject)
        {
            return _store.Remove(id);
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        lock (_lockObject)
        {
            return _store.ContainsKey(id);
        }
    }

    public async Task<int> CountAsync()
    {
        lock (_lockObject)
        {
            return _store.Count;
        }
    }

    /// <summary>
    /// Clears all entities from the repository
    /// </summary>
    public void Clear()
    {
        lock (_lockObject)
        {
            _store.Clear();
        }
    }

    /// <summary>
    /// Gets filtered entities based on a predicate
    /// </summary>
    public async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        lock (_lockObject)
        {
            return _store.Values.Where(predicate).ToList();
        }
    }
}
