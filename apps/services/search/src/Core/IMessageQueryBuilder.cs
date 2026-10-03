using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Core;

/// <summary>
/// Builder interface for creating LINQ-based message queries
/// This builder constructs expression trees that can be translated to SQL
/// by any RDBMS provider or to query DSLs for analytical stores
/// </summary>
public interface IMessageQueryBuilder
{
    // === Content Filtering ===

    /// <summary>
    /// Filter by exact text match in subject or body
    /// </summary>
    IMessageQueryBuilder WithText(string exactText);

    /// <summary>
    /// Filter by text containing the specified term
    /// </summary>
    IMessageQueryBuilder ContainingText(string text);

    /// <summary>
    /// Filter by regex pattern match
    /// </summary>
    IMessageQueryBuilder MatchingPattern(Regex pattern);

    /// <summary>
    /// Filter by wildcard pattern
    /// </summary>
    IMessageQueryBuilder MatchingWildcard(string pattern);

    // === Temporal Filtering ===

    /// <summary>
    /// Filter messages created after the specified date
    /// </summary>
    IMessageQueryBuilder CreatedAfter(DateTimeOffset date);

    /// <summary>
    /// Filter messages created before the specified date
    /// </summary>
    IMessageQueryBuilder CreatedBefore(DateTimeOffset date);

    /// <summary>
    /// Filter messages created within a date range
    /// </summary>
    IMessageQueryBuilder CreatedBetween(DateTimeOffset start, DateTimeOffset end);

    /// <summary>
    /// Filter messages modified after the specified date
    /// </summary>
    IMessageQueryBuilder ModifiedAfter(DateTimeOffset date);

    /// <summary>
    /// Filter messages modified before the specified date
    /// </summary>
    IMessageQueryBuilder ModifiedBefore(DateTimeOffset date);

    // === Participant Filtering ===

    /// <summary>
    /// Filter messages sent from the specified participant
    /// </summary>
    IMessageQueryBuilder FromParticipant(string participantId);

    /// <summary>
    /// Filter messages sent to the specified participant
    /// </summary>
    IMessageQueryBuilder ToParticipant(string participantId);

    /// <summary>
    /// Filter messages involving the specified participant (sender or recipient)
    /// </summary>
    IMessageQueryBuilder Involving(string participantId);

    /// <summary>
    /// Filter messages involving any of the specified participants
    /// </summary>
    IMessageQueryBuilder InvolvingAny(params string[] participantIds);

    /// <summary>
    /// Filter messages involving all of the specified participants
    /// </summary>
    IMessageQueryBuilder InvolvingAll(params string[] participantIds);

    // === Channel/Conversation Filtering ===

    /// <summary>
    /// Filter messages in the specified channel
    /// </summary>
    IMessageQueryBuilder InChannel(string channelId);

    /// <summary>
    /// Filter messages in any of the specified channels
    /// </summary>
    IMessageQueryBuilder InAnyChannel(params string[] channelIds);

    // === Metadata/Status Filtering ===

    /// <summary>
    /// Filter by message status
    /// </summary>
    IMessageQueryBuilder WithStatus(MessageStatus status);

    /// <summary>
    /// Filter by any of the specified statuses
    /// </summary>
    IMessageQueryBuilder WithAnyStatus(params MessageStatus[] statuses);

    /// <summary>
    /// Filter by pinned status
    /// </summary>
    IMessageQueryBuilder IsPinned(bool pinned = true);

    /// <summary>
    /// Filter by attachment presence
    /// </summary>
    IMessageQueryBuilder HasAttachments(bool hasAttachments = true);

    /// <summary>
    /// Filter by link presence
    /// </summary>
    IMessageQueryBuilder HasLinks(bool hasLinks = true);

    /// <summary>
    /// Filter by tag (message must have all specified tags)
    /// </summary>
    IMessageQueryBuilder WithTag(string tag);

    /// <summary>
    /// Filter by any of the specified tags
    /// </summary>
    IMessageQueryBuilder WithAnyTag(params string[] tags);

    /// <summary>
    /// Filter by all of the specified tags
    /// </summary>
    IMessageQueryBuilder WithAllTags(params string[] tags);

    /// <summary>
    /// Filter by size comparison
    /// </summary>
    IMessageQueryBuilder WithSize(SizeComparison comparison, long bytes);

    /// <summary>
    /// Filter by size in bytes
    /// </summary>
    IMessageQueryBuilder WithSizeInBytes(long bytes);

    // === Boolean Composition ===

    /// <summary>
    /// Combine conditions with AND logic
    /// </summary>
    IMessageQueryBuilder And(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder);

    /// <summary>
    /// Combine conditions with OR logic
    /// </summary>
    IMessageQueryBuilder Or(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder);

    /// <summary>
    /// Negate the following conditions
    /// </summary>
    IMessageQueryBuilder Not(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder);

    // === Sorting & Pagination ===

    /// <summary>
    /// Order results by the specified field
    /// </summary>
    IMessageQueryBuilder OrderBy(MessageSortField field, SortDirection direction = SortDirection.Ascending);

    /// <summary>
    /// Secondary sorting
    /// </summary>
    IMessageQueryBuilder ThenBy(MessageSortField field, SortDirection direction = SortDirection.Ascending);

    /// <summary>
    /// Limit the number of results
    /// </summary>
    IMessageQueryBuilder Take(int count);

    /// <summary>
    /// Skip a number of results
    /// </summary>
    IMessageQueryBuilder Skip(int count);

    /// <summary>
    /// Paginate results
    /// </summary>
    IMessageQueryBuilder Page(int pageNumber, int pageSize);

    // === Projection ===

    /// <summary>
    /// Project results to a different type
    /// </summary>
    IMessageQueryBuilder Select<TResult>(Expression<Func<Message, TResult>> selector);

    // === Includes for ORM Navigation Properties ===

    /// <summary>
    /// Include related entities in the query (for ORM lazy loading prevention)
    /// </summary>
    IMessageQueryBuilder Include(string path);

    /// <summary>
    /// Include multiple related entities
    /// </summary>
    IMessageQueryBuilder Include(params string[] paths);

    // === Finalization ===

    /// <summary>
    /// Build the LINQ-based query
    /// </summary>
    IMessageQuery Build();

    /// <summary>
    /// Build and compile the query to a predicate expression
    /// </summary>
    Expression<Func<Message, bool>> BuildPredicate();

    /// <summary>
    /// Resets the current query builder to start fresh
    /// </summary>
    IMessageQueryBuilder Clear();

    /// <summary>
    /// Creates a copy of this builder with the same filters applied
    /// </summary>
    IMessageQueryBuilder Clone();

    /// <summary>
    /// Sets the repository to use for query execution
    /// </summary>
    /// <param name="repository">The repository to use</param>
    IMessageQueryBuilder ForRepository(IRepository<Message> repository);

    /// <summary>
    /// Sets the analytical store adaptor for query translation
    /// </summary>
    /// <param name="adaptor">The analytical store adaptor</param>
    IMessageQueryBuilder ForAnalyticalStore<TAdaptor>(TAdaptor adaptor) where TAdaptor : class;
}