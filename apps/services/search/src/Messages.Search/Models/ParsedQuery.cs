using System.Linq.Expressions;
using Core;

namespace Messages.Search.Models;

/// <summary>
/// Represents a parsed query with all its components
/// </summary>
public class ParsedQuery : IMessageQuery
{
    public string OriginalQuery { get; set; } = string.Empty;
    public List<QueryToken> Tokens { get; set; } = [];
    public List<string> FreeTextTerms { get; set; } = [];
    public List<string> Phrases { get; set; } = [];

    // Parsed filter values
    public List<string> FromParticipants { get; set; } = [];
    public List<string> ToParticipants { get; set; } = [];
    public List<string> InvolvingParticipants { get; set; } = [];
    public List<string> Channels { get; set; } = [];
    public List<MessageStatus> Statuses { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public List<string> AnyTags { get; private init; } = [];
    public List<string> AllTags { get; private init; } = [];
    public List<string> HasFeatures { get; private init; } = [];
    public List<string> ExcludedHasFeatures { get; private init; } = [];
    public TimeRange? CreatedRange { get; set; }
    public TimeRange? ModifiedRange { get; set; }
    public bool? IsPinned { get; set; }
    public SizeFilter? SizeFilter { get; set; }

    // Sorting and pagination
    public List<(MessageSortField Field, SortDirection Direction)> SortFields { get; } = [];
    public Expression<Func<Message, bool>>? Predicate { get; set; }

    public List<(Expression<Func<Message, object>> KeySelector, SortDirection Direction)>
        SortExpressions { get; set; } = [];

    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string[] Includes { get; set; } = [];

    public IMessageQuery And(IMessageQuery other)
        => other switch
        {
            ParsedQuery parsed => new()
            {
                OriginalQuery = $"{OriginalQuery} AND {parsed.OriginalQuery}",
                Predicate = Predicate != null && parsed.Predicate != null
                    ? PredicateBuilder.And(Predicate, parsed.Predicate)
                    : Predicate ?? parsed.Predicate,
                FreeTextTerms = [.. FreeTextTerms.Union(parsed.FreeTextTerms)],
                Phrases = [.. Phrases.Union(parsed.Phrases)],
                FromParticipants = [.. FromParticipants.Union(parsed.FromParticipants)],
                ToParticipants = [.. ToParticipants.Union(parsed.ToParticipants)],
                InvolvingParticipants = [.. InvolvingParticipants.Union(parsed.InvolvingParticipants)],
                Channels = [.. Channels.Union(parsed.Channels)],
                Statuses = [.. Statuses.Union(parsed.Statuses)],
                Tags = [.. Tags.Union(parsed.Tags)],
                AnyTags = [.. AnyTags.Union(parsed.AnyTags)],
                AllTags = [.. AllTags.Union(parsed.AllTags)],
                HasFeatures = [.. HasFeatures.Union(parsed.HasFeatures)],
                ExcludedHasFeatures = [.. ExcludedHasFeatures.Union(parsed.ExcludedHasFeatures)],
                IsPinned = IsPinned ?? parsed.IsPinned,
                Limit = Limit ?? parsed.Limit,
                Offset = Offset ?? parsed.Offset,
                Page = Page ?? parsed.Page,
                PageSize = PageSize ?? parsed.PageSize,
                Includes = [.. Includes, .. parsed.Includes]
            },
            
            _ => this
        };

    public IMessageQuery Or(IMessageQuery other)
        => other switch
        {
            ParsedQuery parsed =>
                new()
                {
                    OriginalQuery = $"{OriginalQuery} OR {parsed.OriginalQuery}",
                    Predicate = Predicate != null && parsed.Predicate != null
                        ? PredicateBuilder.Or(Predicate, parsed.Predicate)
                        : Predicate ?? parsed.Predicate,
                    FreeTextTerms = [.. FreeTextTerms.Union(parsed.FreeTextTerms)],
                    Phrases = [.. Phrases.Union(parsed.Phrases)],
                    FromParticipants = [.. FromParticipants.Union(parsed.FromParticipants)],
                    ToParticipants = [.. ToParticipants.Union(parsed.ToParticipants)],
                    InvolvingParticipants = [.. InvolvingParticipants.Union(parsed.InvolvingParticipants)],
                    Channels = [.. Channels.Union(parsed.Channels)],
                    Statuses = [.. Statuses.Union(parsed.Statuses)],
                    Tags = [.. Tags.Union(parsed.Tags)],
                    AnyTags = [.. AnyTags.Union(parsed.AnyTags)],
                    AllTags = [.. AllTags.Union(parsed.AllTags)],
                    HasFeatures = [.. HasFeatures.Union(parsed.HasFeatures)],
                    ExcludedHasFeatures = [.. ExcludedHasFeatures.Union(parsed.ExcludedHasFeatures)],
                    IsPinned = IsPinned ?? parsed.IsPinned,
                    Limit = Limit ?? parsed.Limit,
                    Offset = Offset ?? parsed.Offset,
                    Page = Page ?? parsed.Page,
                    PageSize = PageSize ?? parsed.PageSize,
                    Includes = [.. Includes, .. parsed.Includes]
                },

            _ => this
        };

    public IMessageQuery Not()
        => new ParsedQuery
        {
            OriginalQuery = $"NOT {OriginalQuery}",
            Predicate = Predicate != null ? PredicateBuilder.Not(Predicate) : null,
            FreeTextTerms = [.. FreeTextTerms],
            Phrases = [.. Phrases],
            FromParticipants = [.. FromParticipants],
            ToParticipants = [.. ToParticipants],
            InvolvingParticipants = [.. InvolvingParticipants],
            Channels = [.. Channels],
            Statuses = [.. Statuses],
            Tags = [.. Tags],
            AnyTags = [.. AnyTags],
            AllTags = [.. AllTags],
            HasFeatures = [.. HasFeatures],
            ExcludedHasFeatures = [.. ExcludedHasFeatures],
            IsPinned = IsPinned,
            Limit = Limit,
            Offset = Offset,
            Page = Page,
            PageSize = PageSize,
            Includes = [.. Includes]
        };

    public IQueryable<Message> ApplyTo(IQueryable<Message> source)
    {
        var query = source;
        var effectivePredicate = Predicate ?? BuildPredicateFromParsedProperties();

        if (effectivePredicate != null)
            query = query.Where(effectivePredicate);

        query = ApplySorting(query);
        query = ApplyPagination(query);

        return query;
    }

    private IQueryable<Message> ApplyPagination(IQueryable<Message> query)
    {
        if (Limit.HasValue)
            return query.Take(Limit.Value);

        if (Page.HasValue && PageSize.HasValue)
            return query.Skip((Page.Value - 1) * PageSize.Value).Take(PageSize.Value);
        
        return query;
    }

    private IQueryable<Message> ApplySorting(IQueryable<Message> query)
    {
        if (SortExpressions.Count > 0)
        {
            var firstSort = SortExpressions[0];
            var orderedQuery = firstSort.Direction == Core.SortDirection.Ascending
                ? query.OrderBy(firstSort.KeySelector)
                : query.OrderByDescending(firstSort.KeySelector);

            for (var i = 1; i < SortExpressions.Count; i++)
            {
                var sort = SortExpressions[i];
                orderedQuery = sort.Direction == Core.SortDirection.Ascending
                    ? orderedQuery.ThenBy(sort.KeySelector)
                    : orderedQuery.ThenByDescending(sort.KeySelector);
            }

            query = orderedQuery;
        }
        else if (SortFields.Count > 0)
        {
            // Apply sort from SortFields (legacy)
            var firstSort = SortFields[0];
            var keySelector = GetSortExpressionFromEnum(firstSort.Field);
            var orderedQuery = firstSort.Direction == Core.SortDirection.Ascending
                ? query.OrderBy(keySelector)
                : query.OrderByDescending(keySelector);

            for (var i = 1; i < SortFields.Count; i++)
            {
                var sort = SortFields[i];
                var nextSelector = GetSortExpressionFromEnum(sort.Field);
                orderedQuery = sort.Direction == Core.SortDirection.Ascending
                    ? orderedQuery.ThenBy(nextSelector)
                    : orderedQuery.ThenByDescending(nextSelector);
            }

            query = orderedQuery;
        }
        else if (!string.IsNullOrEmpty(SortField) && !string.IsNullOrEmpty(SortDirection))
        {
            // Apply legacy sort field/direction
            var isDescending = SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
            var enumDirection = isDescending ? Core.SortDirection.Descending : Core.SortDirection.Ascending;

            var keySelector = GetSortExpression(SortField);
            query = enumDirection == Core.SortDirection.Ascending
                ? query.OrderBy(keySelector)
                : query.OrderByDescending(keySelector);
        }
        else
        {
            // Default sort by CreatedAt descending
            query = query.OrderByDescending(m => m.CreatedAt);
        }

        return query;
    }

    private Expression<Func<Message, bool>>? BuildPredicateFromParsedProperties()
    {
        // Build predicate from old-style properties
        Expression<Func<Message, bool>>? predicate = null;

        predicate = BuildFreeTextTerms(predicate);
        predicate = BuildExactPhraseMatch(predicate);
        predicate = BuildFromParticipants(predicate);
        predicate = BuildToParticipants(predicate);
        predicate = BuildInvolvingParticipants(predicate);

        // Channels
        foreach (var channel in Channels)
        {
            var channelPredicate = PredicateBuilder.Create<Message>(m => m.ChannelId == channel);
            predicate = predicate != null ? PredicateBuilder.And(predicate, channelPredicate) : channelPredicate;
        }

        // Statuses
        if (Statuses.Count > 0)
        {
            var statusPredicate = PredicateBuilder.Create<Message>(m => Statuses.Contains(m.Status));
            predicate = predicate != null ? PredicateBuilder.And(predicate, statusPredicate) : statusPredicate;
        }

        // Tags (all tags must match)
        if (Tags.Count > 0)
        {
            var tagPredicate = PredicateBuilder.Create<Message>(m => Tags.All(t => m.Tags.Contains(t)));
            predicate = predicate != null ? PredicateBuilder.And(predicate, tagPredicate) : tagPredicate;
        }

        // Any tags (any tag must match)
        if (AnyTags.Count > 0)
        {
            var anyTagPredicate = PredicateBuilder.Create<Message>(m => m.Tags.Any(t => AnyTags.Contains(t)));
            predicate = predicate != null ? PredicateBuilder.And(predicate, anyTagPredicate) : anyTagPredicate;
        }

        // All tags
        if (AllTags.Count > 0)
        {
            var allTagPredicate = PredicateBuilder.Create<Message>(m => AllTags.All(t => m.Tags.Contains(t)));
            predicate = predicate != null ? PredicateBuilder.And(predicate, allTagPredicate) : allTagPredicate;
        }

        // Has features
        if (HasFeatures.Count > 0)
        {
            foreach (var feature in HasFeatures)
            {
                Expression<Func<Message, bool>> featurePredicate = feature.ToLower() switch
                {
                    "attachments" => m => m.HasAttachments,
                    "links" => m => m.HasLinks,
                    "pinned" => m => m.IsPinned,
                    _ => m => true
                };
                predicate = predicate != null ? PredicateBuilder.And(predicate, featurePredicate) : featurePredicate;
            }
        }

        // IsPinned
        if (IsPinned.HasValue)
        {
            var pinnedPredicate = PredicateBuilder.Create<Message>(m => m.IsPinned == IsPinned.Value);
            predicate = predicate != null ? PredicateBuilder.And(predicate, pinnedPredicate) : pinnedPredicate;
        }

        // CreatedRange
        if (CreatedRange != null)
        {
            var rangePredicate = PredicateBuilder.Create<Message>(m => true);
            if (CreatedRange.Start.HasValue)
            {
                var startPredicate = PredicateBuilder.Create<Message>(m => m.CreatedAt >= CreatedRange.Start.Value);
                rangePredicate = PredicateBuilder.And(rangePredicate, startPredicate);
            }

            if (CreatedRange.End.HasValue)
            {
                var endPredicate = PredicateBuilder.Create<Message>(m => m.CreatedAt <= CreatedRange.End.Value);
                rangePredicate = PredicateBuilder.And(rangePredicate, endPredicate);
            }

            predicate = predicate != null ? PredicateBuilder.And(predicate, rangePredicate) : rangePredicate;
        }

        // ModifiedRange
        if (ModifiedRange != null)
        {
            var rangePredicate = PredicateBuilder.Create<Message>(m => true);
            if (ModifiedRange.Start.HasValue)
            {
                var startPredicate = PredicateBuilder.Create<Message>(m => m.ModifiedAt >= ModifiedRange.Start.Value);
                rangePredicate = PredicateBuilder.And(rangePredicate, startPredicate);
            }

            if (ModifiedRange.End.HasValue)
            {
                var endPredicate = PredicateBuilder.Create<Message>(m => m.ModifiedAt <= ModifiedRange.End.Value);
                rangePredicate = PredicateBuilder.And(rangePredicate, endPredicate);
            }

            predicate = predicate != null ? PredicateBuilder.And(predicate, rangePredicate) : rangePredicate;
        }

        return predicate;
    }

    private Expression<Func<Message, bool>>? BuildInvolvingParticipants(Expression<Func<Message, bool>>? predicate)
    {
        foreach (var participant in InvolvingParticipants)
        {
            var participantPredicate = PredicateBuilder.Create<Message>(m =>
                m.SenderId == participant || m.RecipientIds.Contains(participant));
            predicate = predicate != null
                ? PredicateBuilder.And(predicate, participantPredicate)
                : participantPredicate;
        }

        return predicate;
    }

    private Expression<Func<Message, bool>>? BuildToParticipants(Expression<Func<Message, bool>>? predicate)
    {
        foreach (var participant in ToParticipants)
        {
            var participantPredicate = PredicateBuilder.Create<Message>(m => m.RecipientIds.Contains(participant));
            predicate = predicate != null
                ? PredicateBuilder.And(predicate, participantPredicate)
                : participantPredicate;
        }

        return predicate;
    }

    private Expression<Func<Message, bool>>? BuildFromParticipants(Expression<Func<Message, bool>>? predicate)
    {
        foreach (var participant in FromParticipants)
        {
            var participantPredicate = PredicateBuilder.Create<Message>(m => m.SenderId == participant);
            predicate = predicate != null
                ? PredicateBuilder.And(predicate, participantPredicate)
                : participantPredicate;
        }

        return predicate;
    }

    private Expression<Func<Message, bool>>? BuildFreeTextTerms(Expression<Func<Message, bool>>? predicate)
    {
        foreach (var term in FreeTextTerms)
        {
            // Use constant expressions to avoid captured variable issues
            var parameter = Expression.Parameter(typeof(Message), "m");
            var termConstant = Expression.Constant(term, typeof(string));
            var comparisonConstant = Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison));

            var content = Expression.Property(parameter, nameof(Message.Content));
            var expression = Expression.Call(
                content,
                typeof(string).GetMethod("Contains", [typeof(string), typeof(StringComparison)])!,
                termConstant,
                comparisonConstant);

            var termPredicate = Expression.Lambda<Func<Message, bool>>(expression, parameter);

            predicate = predicate != null 
                ? PredicateBuilder.And(predicate, termPredicate) 
                : termPredicate;
        }

        return predicate;
    }

    private Expression<Func<Message, bool>>? BuildExactPhraseMatch(Expression<Func<Message, bool>>? predicate)
    {
        foreach (var phrase in Phrases)
        {
            var phrasePredicate = PredicateBuilder
                .Create<Message>(m => m.Content.Equals(phrase, StringComparison.OrdinalIgnoreCase));
            
            predicate = predicate != null
                ? PredicateBuilder.And(predicate, phrasePredicate)
                : phrasePredicate;
        }

        return predicate;
    }

    private static Expression<Func<Message, object>> GetSortExpression(string sortField)
        => sortField switch
        {
            "createdAt" or "created" => m => m.CreatedAt,
            "modifiedAt" or "modified" => m => m.ModifiedAt,
            "sender" => m => m.SenderName,
            "content" => m => m.Content,
            "importance" => m => m.ReactionCount,
            _ => m => m.CreatedAt
        };

    private static Expression<Func<Message, object>> GetSortExpressionFromEnum(MessageSortField field)
        => field switch
        {
            MessageSortField.CreatedAt => m => m.CreatedAt,
            MessageSortField.ModifiedAt => m => m.ModifiedAt,
            MessageSortField.Sender => m => m.SenderName,
            MessageSortField.Content => m => m.Content,
            MessageSortField.Importance => m => m.ReactionCount,
            
            _ => m => m.CreatedAt
        };

    // Boolean structure (for complex queries with AND/OR/NOT)
    public List<QueryClause> Clauses { get; set; } = [];
    public string SortField { get; set; } = string.Empty;
    public string SortDirection { get; set; } = string.Empty;
}