using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Messages.Search;
using Messages.Search.Models;
using Xunit;

namespace Messages.Search.Test;

public class MessageQueryBuilderTests
{
    [Fact]
    public void Build_EmptyBuilder_ReturnsParsedQuery()
    {
        var builder = new MessageQueryBuilder();
        var query = builder.Build();
        
        Assert.IsType<ParsedQuery>(query);
    }

    [Fact]
    public void WithText_AddsPhraseToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.WithText("exact text");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("exact text", query.Phrases);
    }

    [Fact]
    public void ContainingText_AddsFreeTextTermToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.ContainingText("search term");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("search term", query.FreeTextTerms);
    }

    [Fact]
    public void FromParticipant_AddsFromParticipantToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.FromParticipant("user-001");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("user-001", query.FromParticipants);
    }

    [Fact]
    public void ToParticipant_AddsToParticipantToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.ToParticipant("user-002");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("user-002", query.ToParticipants);
    }

    [Fact]
    public void Involving_AddsInvolvingParticipantToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.Involving("user-001");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("user-001", query.InvolvingParticipants);
    }

    [Fact]
    public void InvolvingAny_AddsMultipleInvolvingParticipantsToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.InvolvingAny("user-001", "user-002");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("user-001", query.InvolvingParticipants);
        Assert.Contains("user-002", query.InvolvingParticipants);
    }

    [Fact]
    public void InChannel_AddsChannelToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.InChannel("channel-001");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("channel-001", query.Channels);
    }

    [Fact]
    public void InAnyChannel_AddsMultipleChannelsToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.InAnyChannel("channel-001", "channel-002");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("channel-001", query.Channels);
        Assert.Contains("channel-002", query.Channels);
    }

    [Fact]
    public void WithStatus_AddsStatusToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.WithStatus(MessageStatus.Sent);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains(MessageStatus.Sent, query.Statuses);
    }

    [Fact]
    public void WithAnyStatus_AddsMultipleStatusesToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.WithAnyStatus(MessageStatus.Sent, MessageStatus.Read);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains(MessageStatus.Sent, query.Statuses);
        Assert.Contains(MessageStatus.Read, query.Statuses);
    }

    [Fact]
    public void IsPinned_AddsIsPinnedToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.IsPinned(true);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.True(query.IsPinned);
    }

    [Fact]
    public void HasAttachments_AddsHasFeatureToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.HasAttachments(true);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("attachments", query.HasFeatures);
    }

    [Fact]
    public void WithTag_AddsTagToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.WithTag("urgent");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("urgent", query.Tags);
    }

    [Fact]
    public void WithAnyTag_AddsAnyTagsToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.WithAnyTag("urgent", "important");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("urgent", query.AnyTags);
        Assert.Contains("important", query.AnyTags);
    }

    [Fact]
    public void WithAllTags_AddsAllTagsToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.WithAllTags("urgent", "important");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Contains("urgent", query.AllTags);
        Assert.Contains("important", query.AllTags);
    }

    [Fact]
    public void CreatedAfter_AddsCreatedRangeToQuery()
    {
        var builder = new MessageQueryBuilder();
        var date = DateTimeOffset.UtcNow.AddDays(-1);
        builder.CreatedAfter(date);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.NotNull(query.CreatedRange);
        Assert.Equal(date, query.CreatedRange.Start);
    }

    [Fact]
    public void CreatedBefore_AddsCreatedRangeToQuery()
    {
        var builder = new MessageQueryBuilder();
        var date = DateTimeOffset.UtcNow.AddDays(-1);
        builder.CreatedBefore(date);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.NotNull(query.CreatedRange);
        Assert.Equal(date, query.CreatedRange.End);
    }

    [Fact]
    public void CreatedBetween_AddsCreatedRangeToQuery()
    {
        var builder = new MessageQueryBuilder();
        var start = DateTimeOffset.UtcNow.AddDays(-10);
        var end = DateTimeOffset.UtcNow.AddDays(-1);
        builder.CreatedBetween(start, end);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.NotNull(query.CreatedRange);
        Assert.Equal(start, query.CreatedRange.Start);
        Assert.Equal(end, query.CreatedRange.End);
    }

    [Fact]
    public void ModifiedAfter_AddsModifiedRangeToQuery()
    {
        var builder = new MessageQueryBuilder();
        var date = DateTimeOffset.UtcNow.AddDays(-1);
        builder.ModifiedAfter(date);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.NotNull(query.ModifiedRange);
        Assert.Equal(date, query.ModifiedRange.Start);
    }

    [Fact]
    public void OrderBy_AddsSortFieldToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.OrderBy(MessageSortField.CreatedAt, SortDirection.Descending);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Single(query.SortFields);
        Assert.Equal(MessageSortField.CreatedAt, query.SortFields[0].Field);
        Assert.Equal(SortDirection.Descending, query.SortFields[0].Direction);
    }

    [Fact]
    public void ThenBy_AddsAdditionalSortField()
    {
        var builder = new MessageQueryBuilder();
        builder
            .OrderBy(MessageSortField.CreatedAt, SortDirection.Descending)
            .ThenBy(MessageSortField.Sender, SortDirection.Ascending);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Equal(2, query.SortFields.Count);
        Assert.Equal(MessageSortField.CreatedAt, query.SortFields[0].Field);
        Assert.Equal(SortDirection.Descending, query.SortFields[0].Direction);
        Assert.Equal(MessageSortField.Sender, query.SortFields[1].Field);
        Assert.Equal(SortDirection.Ascending, query.SortFields[1].Direction);
    }

    [Fact]
    public void Take_AddsLimitToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.Take(10);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Equal(10, query.Limit);
    }

    [Fact]
    public void Skip_AddsOffsetToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.Skip(20);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Equal(20, query.Offset);
    }

    [Fact]
    public void Page_AddsPageAndPageSizeToQuery()
    {
        var builder = new MessageQueryBuilder();
        builder.Page(2, 25);
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.Equal(2, query.Page);
        Assert.Equal(25, query.PageSize);
    }

    [Fact]
    public void MatchingPattern_AddsFilter()
    {
        var builder = new MessageQueryBuilder();
        builder.MatchingPattern(new System.Text.RegularExpressions.Regex("test"));
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
    }

    [Fact]
    public void MatchingWildcard_AddsFilter()
    {
        var builder = new MessageQueryBuilder();
        builder.MatchingWildcard("test*");
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
    }

    [Fact]
    public void And_CombinesQueries()
    {
        var builder = new MessageQueryBuilder();
        builder.And(b => b.FromParticipant("user-001").WithStatus(MessageStatus.Sent));
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.NotEmpty(query.Clauses);
    }

    [Fact]
    public void Or_CombinesQueries()
    {
        var builder = new MessageQueryBuilder();
        builder.Or(b => b.FromParticipant("user-001").WithStatus(MessageStatus.Sent));
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.NotEmpty(query.Clauses);
    }

    [Fact]
    public void Not_CombinesQueries()
    {
        var builder = new MessageQueryBuilder();
        builder.Not(b => b.FromParticipant("user-001"));
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
        Assert.NotEmpty(query.Clauses);
    }

    [Fact]
    public void Select_SetSelector()
    {
        var builder = new MessageQueryBuilder();
        builder.Select(m => new { m.Id, m.Subject });
        var query = builder.Build() as ParsedQuery;
        
        Assert.NotNull(query);
    }

    // Note: Internal methods (CompilePredicate, ApplyTo) are not tested directly
    // as they are implementation details. They are tested indirectly through
    // the public SearchAsync method.
}
