using System.Linq.Expressions;

namespace Core;

/// <summary>
/// Service for querying messages using a LINQ-based DSL
/// Can work with any RDBMS repository or analytical data store adaptor
/// </summary>
public interface IMessagesQuerier
{
    /// <summary>
    /// The underlying message repository
    /// </summary>
    IRepository<Message> Repository { get; }

    /// <summary>
    /// Execute a composed query built via DSL
    /// </summary>
    /// <param name="query">The LINQ-based query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of matching messages</returns>
    Task<IReadOnlyList<Message>> SearchAsync(
        IMessageQuery query,
        CancellationToken ct = default);

    /// <summary>
    /// Execute a LINQ expression directly against the repository
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of matching messages</returns>
    Task<IReadOnlyList<Message>> SearchAsync(
        Expression<Func<Message, bool>> predicate,
        CancellationToken ct = default);

    /// <summary>
    /// Simple full-text search across message content
    /// </summary>
    /// <param name="searchText">The text to search for</param>
    /// <param name="options">Search options</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of matching messages</returns>
    Task<IReadOnlyList<Message>> FullTextSearchAsync(
        string searchText,
        FullTextSearchOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Start building a query with the DSL builder
    /// </summary>
    /// <returns>Query builder instance</returns>
    IMessageQueryBuilder Query();

    /// <summary>
    /// Get a message by ID
    /// </summary>
    /// <param name="messageId">The message ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The message or null if not found</returns>
    Task<Message?> GetByIdAsync(string messageId, CancellationToken ct = default);

    /// <summary>
    /// Count messages matching the optional query
    /// </summary>
    /// <param name="query">Optional query to filter by</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Count of matching messages</returns>
    Task<int> CountAsync(IMessageQuery? query = null, CancellationToken ct = default);

    /// <summary>
    /// Count messages matching the predicate
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Count of matching messages</returns>
    Task<int> CountAsync(Expression<Func<Message, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Check if any messages match the optional query
    /// </summary>
    /// <param name="query">Optional query to filter by</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if any messages match</returns>
    Task<bool> AnyAsync(IMessageQuery? query = null, CancellationToken ct = default);

    /// <summary>
    /// Check if any messages match the predicate
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if any messages match</returns>
    Task<bool> AnyAsync(Expression<Func<Message, bool>> predicate, CancellationToken ct = default);

    /// <summary>
    /// Get all messages with optional filtering
    /// </summary>
    /// <param name="predicate">Optional filter predicate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of all matching messages</returns>
    Task<IReadOnlyList<Message>> GetAllAsync(Expression<Func<Message, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>
    /// Execute a query and get a queryable result for further manipulation
    /// </summary>
    /// <param name="query">The LINQ-based query</param>
    /// <returns>Queryable result set</returns>
    IQueryable<Message> AsQueryable(IMessageQuery query);

    /// <summary>
    /// Get the raw queryable from the repository
    /// </summary>
    /// <returns>Raw queryable of messages</returns>
    IQueryable<Message> AsQueryable();
}