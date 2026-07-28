namespace Core;

public interface IMessagesQuerier
{
    // === Primary Search Methods ===

    /// <summary>
    /// Execute a composed query built via DSL
    /// </summary>
    Task<IReadOnlyList<Message>> SearchAsync(
        IMessageQuery query,
        CancellationToken ct = default);

    /// <summary>
    /// Simple full-text search across message content
    /// </summary>
    Task<IReadOnlyList<Message>> FullTextSearchAsync(
        string searchText,
        FullTextSearchOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Start building a query with the DSL builder
    /// </summary>
    IMessageQueryBuilder Query();

    // === Convenience Methods ===

    Task<Message?> GetByIdAsync(string messageId, CancellationToken ct = default);

    Task<int> CountAsync(IMessageQuery? query = null, CancellationToken ct = default);

    Task<bool> AnyAsync(IMessageQuery? query = null, CancellationToken ct = default);
}