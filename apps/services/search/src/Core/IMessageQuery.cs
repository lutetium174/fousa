using System.Linq.Expressions;

namespace Core;

/// <summary>
/// LINQ-based message query interface that can be executed against any RDBMS
/// or analytical data store via expression tree translation
/// </summary>
public interface IMessageQuery
{
    /// <summary>
    /// The compiled predicate expression for filtering messages
    /// </summary>
    Expression<Func<Message, bool>>? Predicate { get; }

    /// <summary>
    /// Sorting specifications for ordering results
    /// </summary>
    List<(Expression<Func<Message, object>> KeySelector, SortDirection Direction)> SortExpressions { get; }

    /// <summary>
    /// Projection selector for transforming results
    /// </summary>
    Expression<Func<Message, object>>? Selector { get; }

    /// <summary>
    /// Maximum number of results to return (null for no limit)
    /// </summary>
    int? Limit { get; }

    /// <summary>
    /// Number of results to skip (null for no offset)
    /// </summary>
    int? Offset { get; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    int? Page { get; }

    /// <summary>
    /// Number of results per page
    /// </summary>
    int? PageSize { get; }

    /// <summary>
    /// Whether to include related entities (for ORM navigation properties)
    /// </summary>
    string[] Includes { get; }

    /// <summary>
    /// Combines this query with another using AND logic
    /// </summary>
    IMessageQuery And(IMessageQuery other);

    /// <summary>
    /// Combines this query with another using OR logic
    /// </summary>
    IMessageQuery Or(IMessageQuery other);

    /// <summary>
    /// Negates this query
    /// </summary>
    IMessageQuery Not();

    /// <summary>
    /// Creates a LINQ queryable from this message query
    /// </summary>
    /// <param name="source">The source queryable</param>
    /// <returns>The filtered and sorted queryable</returns>
    IQueryable<Message> ApplyTo(IQueryable<Message> source);
}