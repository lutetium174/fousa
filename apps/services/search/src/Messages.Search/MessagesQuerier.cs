using System.Linq.Expressions;
using Core;
using Pinotchio;

namespace Messages.Search;

/// <summary>
/// Default implementation of IMessagesQuerier that can work with any RDBMS repository
/// or analytical store adaptor. This is the main querier service that executes LINQ-based
/// queries against the configured data source.
/// </summary>
public class MessagesQuerier : IMessagesQuerier
{
    private readonly IReadRepository<Message>? _repository;
    private readonly Messages.Search.AnalyticalStore.IDataStoreAdaptor<Message>? _adaptor;

    /// <summary>
    /// Creates a new MessagesQuerier with optional repository and analytical store adaptor
    /// </summary>
    /// <param name="repository">Optional RDBMS repository</param>
    /// <param name="adaptor">Optional analytical store adaptor (e.g., for Kafka Pinot)</param>
    public MessagesQuerier(
        IReadRepository<Message>? repository = null,
        Messages.Search.AnalyticalStore.IDataStoreAdaptor<Message>? adaptor = null)
    {
        _repository = repository;
        _adaptor = adaptor;
    }

    /// <summary>
    /// Creates a new MessagesQuerier with an RDBMS repository
    /// </summary>
    /// <param name="repository">The RDBMS repository</param>
    public MessagesQuerier(IReadRepository<Message> repository) : this(repository, null)
    { }

    /// <summary>
    /// Creates a new MessagesQuerier with an analytical store adaptor
    /// </summary>
    /// <param name="adaptor">The analytical store adaptor</param>
    public MessagesQuerier(Messages.Search.AnalyticalStore.IDataStoreAdaptor<Message> adaptor) : this(null, adaptor)
    { }

    /// <summary>
    /// Execute a composed query built via DSL
    /// </summary>
    public async Task<IReadOnlyList<Message>> SearchAsync(
        IMessageQuery query,
        CancellationToken ct = default)
    {
        if (_adaptor != null)
        {
            // Use analytical store adaptor
            var queryable = query.ApplyTo(new List<Message>().AsQueryable());
            return await _adaptor.ExecuteQueryAsync(queryable, ct);
        }
        else if (_repository != null)
        {
            // Use RDBMS repository
            var queryable = query.ApplyTo(_repository.Query());
            return await Task.FromResult(queryable.ToList().AsReadOnly());
        }
        else
        {
            // Fallback to in-memory execution
            var queryable = query.ApplyTo(new List<Message>().AsQueryable());
            return await Task.FromResult(queryable.ToList().AsReadOnly());
        }
    }

    /// <summary>
    /// Execute a LINQ expression directly against the repository
    /// </summary>
    public async Task<IReadOnlyList<Message>> SearchAsync(
        Expression<Func<Message, bool>> predicate,
        CancellationToken ct = default)
    {
        if (_adaptor != null)
        {
            // Use analytical store adaptor
            return await _adaptor.ExecuteQueryAsync(predicate, ct);
        }
        else if (_repository != null)
        {
            // Use RDBMS repository
            var queryable = _repository.Query().Where(predicate);
            return await Task.FromResult(queryable.ToList().AsReadOnly());
        }
        else
        {
            // Fallback to in-memory execution
            var queryable = new List<Message>().AsQueryable().Where(predicate);
            return await Task.FromResult(queryable.ToList().AsReadOnly());
        }
    }

    /// <summary>
    /// Simple full-text search across message content
    /// </summary>
    public async Task<IReadOnlyList<Message>> FullTextSearchAsync(
        string searchText,
        FullTextSearchOptions? options = null,
        CancellationToken ct = default)
    {
        var queryBuilder = new MessageQueryBuilder();
        
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            queryBuilder.ContainingText(searchText);
        }
        
        if (options != null)
        {
            // Apply sorting if specified
            if (options.SortField != MessageSortField.None)
            {
                queryBuilder.OrderBy(options.SortField, options.SortDirection);
            }
            
            // Apply pagination if specified
            if (options.Page.HasValue && options.PageSize.HasValue)
            {
                queryBuilder.Page(options.Page.Value, options.PageSize.Value);
            }
            else if (options.Limit.HasValue)
            {
                queryBuilder.Take(options.Limit.Value);
            }
        }
        
        var query = queryBuilder.Build();
        return await SearchAsync(query, ct);
    }

    /// <summary>
    /// Start building a query with the DSL builder
    /// </summary>
    public IMessageQueryBuilder Query()
    {
        return new MessageQueryBuilder();
    }

    /// <summary>
    /// Get a message by ID
    /// </summary>
    public async Task<Message?> GetByIdAsync(string messageId, CancellationToken ct = default)
    {
        if (_adaptor != null)
        {
            return await _adaptor.GetByIdAsync(messageId, ct);
        }
        else if (_repository != null)
        {
            // For RDBMS, we need to query by ID
            // Note: This is a simple implementation; for production, consider using
            // a proper repository method or FirstOrDefaultAsync with ID predicate
            var queryable = _repository.Query();
            return await Task.FromResult(queryable.FirstOrDefault(m => m.Id == messageId));
        }
        else
        {
            return null; // No data source configured
        }
    }

    /// <summary>
    /// Count messages matching the optional query
    /// </summary>
    public async Task<int> CountAsync(IMessageQuery? query = null, CancellationToken ct = default)
    {
        if (query == null)
        {
            // Count all messages
            if (_adaptor != null)
            {
                return await _adaptor.CountAsync(null, ct);
            }
            else if (_repository != null)
            {
                var queryable = _repository.Query();
                return await Task.FromResult(queryable.Count());
            }
            else
            {
                return 0; // No data source configured
            }
        }
        
        // Count with query filter
        if (_adaptor != null)
        {
            var queryable = query.ApplyTo(new List<Message>().AsQueryable());
            return await _adaptor.CountAsync(queryable, ct);
        }
        else if (_repository != null)
        {
            var queryable = query.ApplyTo(_repository.Query());
            return await Task.FromResult(queryable.Count());
        }
        else
        {
            var queryable = query.ApplyTo(new List<Message>().AsQueryable());
            return await Task.FromResult(queryable.Count());
        }
    }

    /// <summary>
    /// Count messages matching the predicate
    /// </summary>
    public async Task<int> CountAsync(Expression<Func<Message, bool>> predicate, CancellationToken ct = default)
    {
        if (_adaptor != null)
        {
            return await _adaptor.CountAsync(predicate, ct);
        }
        else if (_repository != null)
        {
            var queryable = _repository.Query().Where(predicate);
            return await Task.FromResult(queryable.Count());
        }
        else
        {
            var queryable = new List<Message>().AsQueryable().Where(predicate);
            return await Task.FromResult(queryable.Count());
        }
    }

    /// <summary>
    /// Check if any messages match the optional query
    /// </summary>
    public async Task<bool> AnyAsync(IMessageQuery? query = null, CancellationToken ct = default)
    {
        if (query == null)
        {
            // Check if any messages exist
            if (_adaptor != null)
            {
                return await _adaptor.AnyAsync(null, ct);
            }
            else if (_repository != null)
            {
                var queryable = _repository.Query();
                return await Task.FromResult(queryable.Any());
            }
            else
            {
                return false; // No data source configured
            }
        }
        
        // Check with query filter
        if (_adaptor != null)
        {
            var queryable = query.ApplyTo(new List<Message>().AsQueryable());
            return await _adaptor.AnyAsync(queryable, ct);
        }
        else if (_repository != null)
        {
            var queryable = query.ApplyTo(_repository.Query());
            return await Task.FromResult(queryable.Any());
        }
        else
        {
            var queryable = query.ApplyTo(new List<Message>().AsQueryable());
            return await Task.FromResult(queryable.Any());
        }
    }

    /// <summary>
    /// Check if any messages match the predicate
    /// </summary>
    public async Task<bool> AnyAsync(Expression<Func<Message, bool>> predicate, CancellationToken ct = default)
    {
        if (_adaptor != null)
        {
            return await _adaptor.AnyAsync(predicate, ct);
        }
        else if (_repository != null)
        {
            var queryable = _repository.Query().Where(predicate);
            return await Task.FromResult(queryable.Any());
        }
        else
        {
            var queryable = new List<Message>().AsQueryable().Where(predicate);
            return await Task.FromResult(queryable.Any());
        }
    }

    /// <summary>
    /// Get all messages with optional filtering
    /// </summary>
    public async Task<IReadOnlyList<Message>> GetAllAsync(
        Expression<Func<Message, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        if (_adaptor != null)
        {
            return predicate != null 
                ? await _adaptor.ExecuteQueryAsync(predicate, ct)
                : await _adaptor.ExecuteQueryAsync("*", ct);
        }
        else if (_repository != null)
        {
            var queryable = predicate != null ? _repository.Query().Where(predicate) : _repository.Query();
            return await Task.FromResult(queryable.ToList().AsReadOnly());
        }
        else
        {
            var queryable = predicate != null 
                ? new List<Message>().AsQueryable().Where(predicate)
                : new List<Message>().AsQueryable();
            return await Task.FromResult(queryable.ToList().AsReadOnly());
        }
    }

    /// <summary>
    /// Execute a query and get a queryable result for further manipulation
    /// </summary>
    public IQueryable<Message> AsQueryable(IMessageQuery query)
    {
        if (_repository != null)
        {
            return query.ApplyTo(_repository.Query());
        }
        else
        {
            return query.ApplyTo(new List<Message>().AsQueryable());
        }
    }

    /// <summary>
    /// Get the raw queryable from the repository
    /// </summary>
    public IQueryable<Message> AsQueryable()
    {
        if (_repository != null)
        {
            return _repository.Query();
        }
        else
        {
            return new List<Message>().AsQueryable();
        }
    }
}
