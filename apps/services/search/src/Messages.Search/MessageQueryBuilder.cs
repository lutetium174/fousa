using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Core;

namespace Messages.Search;

/// <summary>
/// Extension methods for IMessageQueryBuilder to work with different data sources
/// </summary>
public static class MessageQueryBuilderExtensions
{
    /// <summary>
    /// Executes the query against the specified repository
    /// </summary>
    public static async Task<IReadOnlyList<Message>> ExecuteAsync(
        this IMessageQueryBuilder builder,
        IRepository<Message> repository,
        CancellationToken ct = default)
    {
        var query = builder.Build();
        var queryable = query.ApplyTo(repository.Query());

        // For in-memory collections, execute immediately
        if (queryable is IEnumerable<Message> enumerable)
        {
            await Task.CompletedTask;
            return enumerable.ToList().AsReadOnly();
        }

        // For RDBMS, materialize the results
        return await Task.FromResult(queryable.ToList().AsReadOnly());
    }

    /// <summary>
    /// Translates the query to the specified analytical store's query language
    /// </summary>
    public static string TranslateTo<TAdaptor>(this IMessageQueryBuilder builder, TAdaptor adaptor)
        where TAdaptor : Messages.Search.AnalyticalStore.IAnalyticalStoreAdaptor<Message>
        => builder.Build() is not LinqMessageQuery linqQuery
            ? string.Empty
            : adaptor.TranslateQuery(linqQuery.ApplyTo(new List<Message>().AsQueryable()));
}

/// <summary>
/// LINQ-based message query builder that constructs expression trees
/// for query translation to SQL by RDBMS providers
/// </summary>
public class MessageQueryBuilder : IMessageQueryBuilder
{
    private readonly LinqMessageQuery _query = new();
    private List<string> _includes = new();
    private List<Expression<Func<Message, bool>>> _currentContext = new();
    private bool _isNegatedContext = false;

    public MessageQueryBuilder()
    {
    }

    // === Content Filtering ===

    public IMessageQueryBuilder WithText(string exactText)
    {
        var predicate = CreateTextPredicate(m =>
            m.Subject.Equals(exactText, StringComparison.OrdinalIgnoreCase) ||
            m.Body.Equals(exactText, StringComparison.OrdinalIgnoreCase));
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder ContainingText(string text)
    {
        var predicate = CreateTextPredicate(m =>
            m.Subject.Contains(text, StringComparison.OrdinalIgnoreCase) ||
            m.Body.Contains(text, StringComparison.OrdinalIgnoreCase));
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder MatchingPattern(Regex pattern)
    {
        var predicate = CreateTextPredicate(m =>
            pattern.IsMatch(m.Subject) || pattern.IsMatch(m.Body));
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder MatchingWildcard(string pattern)
    {
        // Convert wildcard to regex
        var regexPattern = pattern
            .Replace("*", ".*")
            .Replace("?", ".")
            .Replace("[", "[")
            .Replace("]", "]");

        try
        {
            var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
            return MatchingPattern(regex);
        }
        catch
        {
            // Fallback to contains if regex fails
            return ContainingText(pattern.Replace("*", "").Replace("?", ""));
        }
    }

    // === Temporal Filtering ===

    public IMessageQueryBuilder CreatedAfter(DateTimeOffset date)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.CreatedAt >= date);
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder CreatedBefore(DateTimeOffset date)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.CreatedAt <= date);
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder CreatedBetween(DateTimeOffset start, DateTimeOffset end)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.CreatedAt >= start && m.CreatedAt <= end);
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder ModifiedAfter(DateTimeOffset date)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.ModifiedAt >= date);
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder ModifiedBefore(DateTimeOffset date)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.ModifiedAt <= date);
        AddPredicate(predicate);
        return this;
    }

    // === Participant Filtering ===

    public IMessageQueryBuilder FromParticipant(string participantId)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.SenderId == participantId);
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder ToParticipant(string participantId)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.RecipientIds.Contains(participantId));
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder Involving(string participantId)
    {
        var predicate = PredicateBuilder.Create<Message>(m =>
            m.SenderId == participantId || m.RecipientIds.Contains(participantId));
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder InvolvingAny(params string[] participantIds)
    {
        if (participantIds.Length == 0) return this;

        var predicate = PredicateBuilder.Create<Message>(m =>
            participantIds.Contains(m.SenderId) ||
            m.RecipientIds.Any(r => participantIds.Contains(r)));
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder InvolvingAll(params string[] participantIds)
    {
        if (participantIds.Length == 0) return this;

        var predicate = PredicateBuilder.Create<Message>(m =>
            participantIds.All(p =>
                m.SenderId == p || m.RecipientIds.Contains(p)));
        AddPredicate(predicate);
        return this;
    }

    // === Channel/Conversation Filtering ===

    public IMessageQueryBuilder InChannel(string channelId)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.ChannelId == channelId);
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder InAnyChannel(params string[] channelIds)
    {
        if (channelIds.Length == 0) return this;

        var predicate = PredicateBuilder.Create<Message>(m => channelIds.Contains(m.ChannelId));
        AddPredicate(predicate);
        return this;
    }

    // === Metadata/Status Filtering ===

    public IMessageQueryBuilder WithStatus(MessageStatus status)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.Status == status);
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder WithAnyStatus(params MessageStatus[] statuses)
    {
        if (statuses.Length == 0) return this;

        var predicate = PredicateBuilder.Create<Message>(m => statuses.Contains(m.Status));
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder IsPinned(bool pinned = true)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.IsPinned == pinned);
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder HasAttachments(bool hasAttachments = true)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.HasAttachments == hasAttachments);
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder HasLinks(bool hasLinks = true)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.HasLinks == hasLinks);
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder WithTag(string tag)
    {
        var predicate = PredicateBuilder.Create<Message>(m => m.Tags.Contains(tag));
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder WithAnyTag(params string[] tags)
    {
        if (tags.Length == 0) return this;

        var predicate = PredicateBuilder.Create<Message>(m =>
            m.Tags.Any(t => tags.Contains(t)));
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder WithAllTags(params string[] tags)
    {
        if (tags.Length == 0) return this;

        var predicate = PredicateBuilder.Create<Message>(m =>
            tags.All(t => m.Tags.Contains(t)));
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder WithSize( Core.SizeComparison comparison, long bytes)
    {
        Expression<Func<Message, bool>> predicate = comparison switch
        {
             Core.SizeComparison.Equal => m => m.SizeInBytes == bytes,
             Core.SizeComparison.GreaterThan => m => m.SizeInBytes > bytes,
             Core.SizeComparison.GreaterThanOrEqual => m => m.SizeInBytes >= bytes,
             Core.SizeComparison.LessThan => m => m.SizeInBytes < bytes,
             Core.SizeComparison.LessThanOrEqual => m => m.SizeInBytes <= bytes,
            _ => m => m.SizeInBytes == bytes
        };
        AddPredicate(predicate);
        return this;
    }

    public IMessageQueryBuilder WithSizeInBytes(long bytes)
    {
        return WithSize( Core.SizeComparison.Equal, bytes);
    }

    // === Boolean Composition ===

    public IMessageQueryBuilder And(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder)
    {
        // Save current context
        var savedPredicates = new List<Expression<Func<Message, bool>>>(_currentContext);
        var savedNegation = _isNegatedContext;

        // Start new AND context
        _currentContext.Clear();

        // Build the sub-query
        var subBuilder = new MessageQueryBuilder();
        builder(subBuilder);
        var subQuery = subBuilder.Build() as LinqMessageQuery;

        if (subQuery != null)
        {
            // Get the sub-query predicate
            subQuery.CompilePredicate();
            if (subQuery.Predicate != null)
            {
                _currentContext.Add(subQuery.Predicate);
            }
        }

        // Restore and combine
        _currentContext.InsertRange(0, savedPredicates);
        _isNegatedContext = savedNegation;

        return this;
    }

    public IMessageQueryBuilder Or(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder)
    {
        // Build the sub-query
        var subBuilder = new MessageQueryBuilder();
        builder(subBuilder);
        var subQuery = subBuilder.Build() as LinqMessageQuery;

        if (subQuery != null)
        {
            // Get the sub-query predicate
            subQuery.CompilePredicate();
            if (subQuery.Predicate != null)
            {
                // For OR, we need to add the current predicates as OR conditions
                var orPredicate = CombinePredicatesWithOr(_currentContext, subQuery.Predicate);
                _currentContext.Clear();
                _currentContext.Add(orPredicate);
            }
        }

        return this;
    }

    public IMessageQueryBuilder Not(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder)
    {
        // Save current negation state
        var savedNegation = _isNegatedContext;

        // Start negated context
        _isNegatedContext = true;

        // Build the sub-query
        var subBuilder = new MessageQueryBuilder();
        builder(subBuilder);
        var subQuery = subBuilder.Build() as LinqMessageQuery;

        if (subQuery != null)
        {
            // Get the sub-query predicate
            subQuery.CompilePredicate();
            if (subQuery.Predicate != null)
            {
                // Negate the predicate
                var negated = PredicateBuilder.Not(subQuery.Predicate);
                _currentContext.Add(negated);
            }
        }

        // Restore negation state
        _isNegatedContext = savedNegation;

        return this;
    }

    private Expression<Func<Message, bool>> CombinePredicatesWithOr(
        List<Expression<Func<Message, bool>>> predicates,
        Expression<Func<Message, bool>> additionalPredicate)
    {
        if (predicates.Count == 0)
            return additionalPredicate;

        if (predicates.Count == 1)
        {
            return PredicateBuilder.Or(predicates[0], additionalPredicate);
        }

        // Combine all predicates with OR
        var combined = predicates[0];
        for (int i = 1; i < predicates.Count; i++)
        {
            combined = PredicateBuilder.Or(combined, predicates[i]);
        }

        return PredicateBuilder.Or(combined, additionalPredicate);
    }

    // === Sorting & Pagination ===

    public IMessageQueryBuilder OrderBy(MessageSortField field, SortDirection direction = SortDirection.Ascending)
    {
        var keySelector = GetSortExpression(field);
        _query.SetSorting(new List<(Expression<Func<Message, object>> KeySelector, SortDirection Direction)>
        {
            (keySelector, direction)
        });
        return this;
    }

    public IMessageQueryBuilder ThenBy(MessageSortField field, SortDirection direction = SortDirection.Ascending)
    {
        var keySelector = GetSortExpression(field);
        _query.ThenSort(keySelector, direction);
        return this;
    }

    public IMessageQueryBuilder Take(int count)
    {
        _query.SetPagination(_query.Limit, _query.Offset, _query.Page, count);
        return this;
    }

    public IMessageQueryBuilder Skip(int count)
    {
        _query.SetPagination(_query.Limit, count, _query.Page, _query.PageSize);
        return this;
    }

    public IMessageQueryBuilder Page(int pageNumber, int pageSize)
    {
        _query.SetPagination(null, null, pageNumber, pageSize);
        return this;
    }

    // === Projection ===

    public IMessageQueryBuilder Select<TResult>(Expression<Func<Message, TResult>> selector)
    {
        _query.SetSelector(selector);
        return this;
    }

    // === Includes for ORM Navigation Properties ===

    public IMessageQueryBuilder Include(string path)
    {
        if (!_includes.Contains(path))
            _includes.Add(path);
        return this;
    }

    public IMessageQueryBuilder Include(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (!_includes.Contains(path))
                _includes.Add(path);
        }

        return this;
    }

    // === Finalization ===

    public IMessageQuery Build()
    {
        // Compile all predicates
        CompileAllPredicates();
        _query.SetIncludes(_includes.ToArray());
        return _query;
    }

    public Expression<Func<Message, bool>> BuildPredicate()
    {
        Build();
        return _query.Predicate ?? (m => true);
    }

    public IMessageQueryBuilder Clear()
    {
        _currentContext.Clear();
        _isNegatedContext = false;
        _includes.Clear();
        // Note: We don't clear _query because it might be reused
        // Instead, we create a new query state
        return this;
    }

    public IMessageQueryBuilder Clone()
    {
        var clone = new MessageQueryBuilder();
        // Copy the current state
        clone._currentContext.AddRange(_currentContext);
        clone._isNegatedContext = _isNegatedContext;
        clone._includes.AddRange(_includes);
        return clone;
    }

    public IMessageQueryBuilder ForRepository(IRepository<Message> repository)
    {
        // Note: The repository is not stored in the builder itself
        // This method is here for interface completeness
        // The actual repository is used when building and executing the query
        return this;
    }

    public IMessageQueryBuilder ForAnalyticalStore<TAdaptor>(TAdaptor adaptor) where TAdaptor : class
    {
        // Note: The adaptor is not stored in the builder itself
        // This method is here for interface completeness
        // The actual adaptor is used when translating the query
        return this;
    }

    private void CompileAllPredicates()
    {
        // Add all current context predicates to the query
        foreach (var predicate in _currentContext)
        {
            _query.AddPredicate(predicate, true);
        }

        _query.CompilePredicate();
    }

    private void AddPredicate(Expression<Func<Message, bool>> predicate)
    {
        if (_isNegatedContext)
        {
            _currentContext.Add(PredicateBuilder.Not(predicate));
        }
        else
        {
            _currentContext.Add(predicate);
        }
    }

    private Expression<Func<Message, bool>> CreateTextPredicate(Expression<Func<Message, bool>> basePredicate)
    {
        return basePredicate;
    }

    private Expression<Func<Message, object>> GetSortExpression(MessageSortField field)
    {
        return field switch
        {
            MessageSortField.CreatedAt => m => m.CreatedAt,
            MessageSortField.ModifiedAt => m => m.ModifiedAt,
            MessageSortField.Sender => m => m.SenderName,
            MessageSortField.Subject => m => m.Subject,
            MessageSortField.Importance => m => m.ReactionCount,
            _ => m => m.CreatedAt
        };
    }
}