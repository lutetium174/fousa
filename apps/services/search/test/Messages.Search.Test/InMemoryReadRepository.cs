using System.Linq.Expressions;
using Core;

namespace Messages.Search.Test;

/// <summary>
/// In-memory implementation of IRepository for testing and development
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public class InMemoryRepository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly List<TEntity> _entities = new();
    private readonly Dictionary<string, TEntity> _entityById = new();
    private readonly Func<TEntity, string> _idSelector;

    /// <summary>
    /// Creates a new in-memory repository
    /// </summary>
    /// <param name="entities">Initial entities</param>
    /// <param name="idSelector">Function to extract ID from entity</param>
    public InMemoryRepository(
        IEnumerable<TEntity>? entities = null,
        Func<TEntity, string>? idSelector = null)
    {
        _idSelector = idSelector ?? GetDefaultIdSelector();
        
        if (entities != null)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }
        }
    }

    private Func<TEntity, string> GetDefaultIdSelector()
    {
        // Try to use Id property
        var idProperty = typeof(TEntity).GetProperty("Id");
        if (idProperty != null && idProperty.PropertyType == typeof(string))
        {
            return entity => (string)idProperty.GetValue(entity)!;
        }
        
        // Try to use ID property
        var idProp = typeof(TEntity).GetProperty("ID");
        if (idProp != null && idProp.PropertyType == typeof(string))
        {
            return entity => (string)idProp.GetValue(entity)!;
        }
        
        // Fallback to ToString()
        return entity => entity.ToString() ?? Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Gets the queryable collection
    /// </summary>
    public IQueryable<TEntity> Query()
    {
        return _entities.AsQueryable();
    }

    /// <summary>
    /// Gets entities by predicate
    /// </summary>
    public IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
    {
        return _entities.AsQueryable().Where(predicate);
    }

    /// <summary>
    /// Gets an entity by ID
    /// </summary>
    public async Task<TEntity?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return _entityById.TryGetValue(id, out var entity) ? entity : null;
    }

    /// <summary>
    /// Gets all entities
    /// </summary>
    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return _entities.AsReadOnly();
    }

    /// <summary>
    /// Counts entities matching the optional predicate
    /// </summary>
    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return predicate == null ? _entities.Count : _entities.Count(predicate.Compile());
    }

    /// <summary>
    /// Checks if any entity matches the optional predicate
    /// </summary>
    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return predicate == null ? _entities.Any() : _entities.Any(predicate.Compile());
    }

    /// <summary>
    /// Gets the first entity matching the predicate or null
    /// </summary>
    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return predicate == null 
            ? _entities.FirstOrDefault()
            : _entities.FirstOrDefault(predicate.Compile());
    }

    /// <summary>
    /// Adds an entity to the repository
    /// </summary>
    public void Add(TEntity entity)
    {
        var id = _idSelector(entity);
        _entities.Add(entity);
        _entityById[id] = entity;
    }

    /// <summary>
    /// Adds multiple entities to the repository
    /// </summary>
    public void AddRange(IEnumerable<TEntity> entities)
    {
        foreach (var entity in entities)
        {
            Add(entity);
        }
    }

    /// <summary>
    /// Removes an entity from the repository
    /// </summary>
    public bool Remove(TEntity entity)
    {
        var id = _idSelector(entity);
        var removedFromList = _entities.Remove(entity);
        var removedFromDict = _entityById.Remove(id);
        return removedFromList && removedFromDict;
    }

    /// <summary>
    /// Removes an entity by ID
    /// </summary>
    public bool Remove(string id)
    {
        if (_entityById.TryGetValue(id, out var entity))
        {
            return Remove(entity);
        }
        return false;
    }

    /// <summary>
    /// Updates an entity in the repository
    /// </summary>
    public void Update(TEntity entity)
    {
        var id = _idSelector(entity);
        if (_entityById.ContainsKey(id))
        {
            _entityById[id] = entity;
        }
    }

    /// <summary>
    /// Clears all entities from the repository
    /// </summary>
    public void Clear()
    {
        _entities.Clear();
        _entityById.Clear();
    }

    /// <summary>
    /// Gets all entities as a list
    /// </summary>
    public IReadOnlyList<TEntity> GetAll() => _entities.AsReadOnly();
}
