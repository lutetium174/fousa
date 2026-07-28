using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Core;

public interface IMessageQueryBuilder
{
    // === Content Filtering ===

    IMessageQueryBuilder WithText(string exactText);
    IMessageQueryBuilder ContainingText(string text);
    IMessageQueryBuilder MatchingPattern(Regex pattern);
    IMessageQueryBuilder MatchingWildcard(string pattern);

    // === Temporal Filtering ===

    IMessageQueryBuilder CreatedAfter(DateTimeOffset date);
    IMessageQueryBuilder CreatedBefore(DateTimeOffset date);
    IMessageQueryBuilder CreatedBetween(DateTimeOffset start, DateTimeOffset end);
    IMessageQueryBuilder ModifiedAfter(DateTimeOffset date);

    // === Participant Filtering ===

    IMessageQueryBuilder FromParticipant(string participantId);
    IMessageQueryBuilder ToParticipant(string participantId);
    IMessageQueryBuilder Involving(string participantId);
    IMessageQueryBuilder InvolvingAny(params string[] participantIds);
    IMessageQueryBuilder InvolvingAll(params string[] participantIds);

    // === Channel/Conversation Filtering ===

    IMessageQueryBuilder InChannel(string channelId);
    IMessageQueryBuilder InAnyChannel(params string[] channelIds);

    // === Metadata/Status Filtering ===

    IMessageQueryBuilder WithStatus(MessageStatus status);
    IMessageQueryBuilder WithAnyStatus(params MessageStatus[] statuses);
    IMessageQueryBuilder IsPinned(bool pinned = true);
    IMessageQueryBuilder HasAttachments(bool hasAttachments = true);
    IMessageQueryBuilder WithTag(string tag);
    IMessageQueryBuilder WithAnyTag(params string[] tags);
    IMessageQueryBuilder WithAllTags(params string[] tags);

    // === Boolean Composition ===

    IMessageQueryBuilder And(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder);
    IMessageQueryBuilder Or(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder);
    IMessageQueryBuilder Not(Func<IMessageQueryBuilder, IMessageQueryBuilder> builder);

    // === Sorting & Pagination ===

    IMessageQueryBuilder OrderBy(MessageSortField field, SortDirection direction = SortDirection.Ascending);
    IMessageQueryBuilder ThenBy(MessageSortField field, SortDirection direction = SortDirection.Ascending);

    IMessageQueryBuilder Take(int count);
    IMessageQueryBuilder Skip(int count);
    IMessageQueryBuilder Page(int pageNumber, int pageSize);

    // === Projection ===

    IMessageQueryBuilder Select<TResult>(Expression<Func<Message, TResult>> selector);

    // === Finalization ===

    IMessageQuery Build();
}