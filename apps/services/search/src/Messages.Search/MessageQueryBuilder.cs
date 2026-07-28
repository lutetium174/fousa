using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Core;

namespace Messages.Search;

public class MessageQueryBuilder : IMessageQueryBuilder
{
    private readonly ParsedQuery _parsedQuery = new();
    private readonly List<Expression<Func<Core.Message, bool>>> _filters = new();
    private readonly List<(MessageSortField Field, SortDirection Direction)> _sortFields = new();
    private int? _takeCount;
    private int? _skipCount;

    public MessageQueryBuilder()
    {
    }

    public MessageQueryBuilder(ParsedQuery parsedQuery)
    {
        _parsedQuery = parsedQuery;
    }

    // === Content Filtering ===

    public IMessageQueryBuilder WithText(string exactText)
    {
        _parsedQuery.Phrases.Add(exactText);
        _filters.Add(m => m.Subject.Contains(exactText) || m.Body.Contains(exactText));
        return this;
    }

    public IMessageQueryBuilder ContainingText(string text)
    {
        _parsedQuery.FreeTextTerms.Add(text);
        _filters.Add(m => m.Subject.Contains(text) || m.Body.Contains(text));
        return this;
    }

    public IMessageQueryBuilder MatchingPattern(Regex pattern)
    {
        _filters.Add(m => pattern.IsMatch(m.Subject) || pattern.IsMatch(m.Body));
        return this;
    }

    public IMessageQueryBuilder MatchingWildcard(string pattern)
    {
        var regexPattern = pattern
            .Replace("*", ".*")
            .Replace("?", ".")
            .Replace(".", "\\.");
        var regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
        _filters.Add(m => regex.IsMatch(m.Subject) || regex.IsMatch(m.Body));
        return this;
    }

    // === Temporal Filtering ===

    public IMessageQueryBuilder CreatedAfter(DateTimeOffset date)
    {
        _parsedQuery.CreatedRange = new TimeRange { Start = date };
        _filters.Add(m => m.CreatedAt >= date);
        return this;
    }

    public IMessageQueryBuilder CreatedBefore(DateTimeOffset date)
    {
        _parsedQuery.CreatedRange = new TimeRange { End = date };
        _filters.Add(m => m.CreatedAt <= date);
        return this;
    }

    public IMessageQueryBuilder CreatedBetween(DateTimeOffset start, DateTimeOffset end)
    {
        _parsedQuery.CreatedRange = new TimeRange { Start = start, End = end };
        _filters.Add(m => m.CreatedAt >= start && m.CreatedAt <= end);
        return this;
    }

    public IMessageQueryBuilder ModifiedAfter(DateTimeOffset date)
    {
        _parsedQuery.ModifiedRange = new TimeRange { Start = date };
        _filters.Add(m => m.ModifiedAt >= date);
        return this;
    }

    // === Participant Filtering ===

    public IMessageQueryBuilder FromParticipant(string participantId)
    {
        _parsedQuery.FromParticipants.Add(participantId);
        _filters.Add(m => m.SenderId == participantId);
        return this;
    }

    public IMessageQueryBuilder ToParticipant(string participantId)
    {
        _parsedQuery.ToParticipants.Add(participantId);
        _filters.Add(m => m.RecipientIds.Contains(participantId));
        return this;
    }

    public IMessageQueryBuilder Involving(string participantId)
    {
        _parsedQuery.InvolvingParticipants.Add(participantId);
        _filters.Add(m => m.SenderId == participantId || m.RecipientIds.Contains(participantId));
        return this;
    }

    public IMessageQueryBuilder InvolvingAny(params string[] participantIds)
    {
        foreach (var id in participantIds)
            _parsedQuery.InvolvingParticipants.Add(id);
        _filters.Add(m => participantIds.Contains(m.SenderId) || m.RecipientIds.Any(participantIds.Contains));
        return this;
    }

    public IMessageQueryBuilder InvolvingAll(params string[] participantIds)
    {
        foreach (var id in participantIds)
            _parsedQuery.InvolvingParticipants.Add(id);
        _filters.Add(m => 
            participantIds.All(id => m.SenderId == id || m.RecipientIds.Contains(id)));
        return this;
    }

    // === Channel/Conversation Filtering ===

    public IMessageQueryBuilder InChannel(string channelId)
    {
        _parsedQuery.Channels.Add(channelId);
        _filters.Add(m => m.ChannelId == channelId);
        return this;
    }

    public IMessageQueryBuilder InAnyChannel(params string[] channelIds)
    {
        foreach (var id in channelIds)
            _parsedQuery.Channels.Add(id);
        _filters.Add(m => channelIds.Contains(m.ChannelId));
        return this;
    }

    // === Metadata/Status Filtering ===

    public IMessageQueryBuilder WithStatus(MessageStatus status)
    {
        _parsedQuery.Statuses.Add(status);
        _filters.Add(m => m.Status == status);
        return this;
    }

    public IMessageQueryBuilder WithAnyStatus(params MessageStatus[] statuses)
    {
        foreach (var status in statuses)
            _parsedQuery.Statuses.Add(status);
        _filters.Add(m => statuses.Contains(m.Status));
        return this;
    }

    public IMessageQueryBuilder IsPinned(bool pinned = true)
    {
        _parsedQuery.IsPinned = pinned;
        _filters.Add(m => m.IsPinned == pinned);
        return this;
    }

    public IMessageQueryBuilder HasAttachments(bool hasAttachments = true)
    {
        _parsedQuery.HasFeatures.Add("attachments");
        _filters.Add(m => m.HasAttachments == hasAttachments);
        return this;
    }

    public IMessageQueryBuilder WithTag(string tag)
    {
        _parsedQuery.Tags.Add(tag);
        _filters.Add(m => m.Tags.Contains(tag));
        return this;
    }

    public IMessageQueryBuilder WithAnyTag(params string[] tags)
    {
        foreach (var tag in tags)
            _parsedQuery.AnyTags.Add(tag);
        _filters.Add(m => m.Tags.Any(tags.Contains));
        return this;
    }

    public IMessageQueryBuilder WithAllTags(params string[] tags)
    {
        foreach (var tag in tags)
            _parsedQuery.AllTags.Add(tag);
        _filters.Add(m => tags.All(m.Tags.Contains));
        return this;
    }

    // === Boolean Composition ===

    public IMessageQueryBuilder And(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder)
    {
        var subBuilder = new MessageQueryBuilder();
        builder(subBuilder);
        var subFilter = subBuilder.Build() as ParsedQuery;
        if (subFilter != null)
        {
            // Combine filters with AND
            var andClause = new QueryClause { Operator = BooleanOperator.And };
            andClause.Tokens.AddRange(subFilter.Tokens);
            _parsedQuery.Clauses.Add(andClause);
            _filters.AddRange(subBuilder._filters);
        }
        return this;
    }

    public IMessageQueryBuilder Or(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder)
    {
        var subBuilder = new MessageQueryBuilder();
        builder(subBuilder);
        var subFilter = subBuilder.Build() as ParsedQuery;
        if (subFilter != null)
        {
            // Combine filters with OR
            var orClause = new QueryClause { Operator = BooleanOperator.Or };
            orClause.Tokens.AddRange(subFilter.Tokens);
            _parsedQuery.Clauses.Add(orClause);
            // For OR, we need to add a disjunction to filters
            var subFilters = subBuilder._filters;
            if (subFilters.Count > 0 && _filters.Count > 0)
            {
                // Combine existing filters with OR
                var existingFilters = _filters.ToArray();
                var newFilters = subFilters.ToArray();
                _filters.Clear();
                _filters.Add(m => existingFilters.Any(f => f.Compile()(m)) || newFilters.Any(f => f.Compile()(m)));
            }
            else
            {
                _filters.AddRange(subFilters);
            }
        }
        return this;
    }

    public IMessageQueryBuilder Not(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder)
    {
        var subBuilder = new MessageQueryBuilder();
        builder(subBuilder);
        var subFilter = subBuilder.Build() as ParsedQuery;
        if (subFilter != null)
        {
            var notClause = new QueryClause { Operator = BooleanOperator.And, IsNegated = true };
            notClause.Tokens.AddRange(subFilter.Tokens);
            _parsedQuery.Clauses.Add(notClause);
            // Negate the sub filters
            var subFilters = subBuilder._filters;
            if (subFilters.Count > 0)
            {
                _filters.Add(m => !subFilters.Any(f => f.Compile()(m)));
            }
        }
        return this;
    }

    // === Sorting & Pagination ===

    public IMessageQueryBuilder OrderBy(MessageSortField field, SortDirection direction = SortDirection.Ascending)
    {
        _parsedQuery.SortField = field;
        _parsedQuery.SortDirection = direction;
        _sortFields.Add((field, direction));
        return this;
    }

    public IMessageQueryBuilder ThenBy(MessageSortField field, SortDirection direction = SortDirection.Ascending)
    {
        _sortFields.Add((field, direction));
        return this;
    }

    public IMessageQueryBuilder Take(int count)
    {
        _takeCount = count;
        _parsedQuery.Limit = count;
        return this;
    }

    public IMessageQueryBuilder Skip(int count)
    {
        _skipCount = count;
        _parsedQuery.Offset = count;
        return this;
    }

    public IMessageQueryBuilder Page(int pageNumber, int pageSize)
    {
        _skipCount = (pageNumber - 1) * pageSize;
        _takeCount = pageSize;
        _parsedQuery.Page = pageNumber;
        _parsedQuery.PageSize = pageSize;
        return this;
    }

    // === Projection ===

    public IMessageQueryBuilder Select<TResult>(Expression<Func<Core.Message, TResult>> selector)
    {
        // Projection is not implemented in this version
        // The selector is stored for future use
        return this;
    }

    // === Finalization ===

    Core.IMessageQuery IMessageQueryBuilder.Build()
    {
        return _parsedQuery;
    }

    public Core.IMessageQuery Build()
    {
        return _parsedQuery;
    }

    // Helper method to compile the query into a predicate
    internal Func<Core.Message, bool> CompilePredicate()
    {
        if (_filters.Count == 0)
            return _ => true;
        
        var compiledFilters = _filters.Select(f => f.Compile()).ToArray();
        return m => compiledFilters.All(f => f(m));
    }

    internal IEnumerable<Core.Message> ApplyTo(IEnumerable<Core.Message> messages)
    {
        var predicate = CompilePredicate();
        var filtered = messages.Where(predicate);
        
        // Apply sorting
        IOrderedEnumerable<Core.Message>? ordered = null;
        if (_sortFields.Count > 0)
        {
            var firstSort = _sortFields[0];
            var firstSortKey = GetSortKey(firstSort.Field);
            ordered = firstSort.Direction == SortDirection.Ascending
                ? filtered.OrderBy(firstSortKey)
                : filtered.OrderByDescending(firstSortKey);
            
            for (int i = 1; i < _sortFields.Count; i++)
            {
                var sort = _sortFields[i];
                var sortKey = GetSortKey(sort.Field);
                ordered = sort.Direction == SortDirection.Ascending
                    ? ordered.ThenBy(sortKey)
                    : ordered.ThenByDescending(sortKey);
            }
            
            filtered = ordered;
        }
        
        // Apply pagination
        if (_skipCount.HasValue)
            filtered = filtered.Skip(_skipCount.Value);
        if (_takeCount.HasValue)
            filtered = filtered.Take(_takeCount.Value);
        
        return filtered.ToList();
    }

    private Func<Core.Message, object> GetSortKey(MessageSortField field)
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
