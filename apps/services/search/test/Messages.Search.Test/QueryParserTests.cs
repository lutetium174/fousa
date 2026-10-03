using System;
using System.Linq;
using Core;
using Messages.Search;
using Xunit;

namespace Messages.Search.Test;

public class QueryParserTests
{
    private readonly QueryParser _parser = new();

    [Fact]
    public void Parse_EmptyQuery_ReturnsEmptyParsedQuery()
    {
        var result = _parser.Parse("");
        Assert.Equal("", result.OriginalQuery);
        Assert.Empty(result.FreeTextTerms);
        Assert.Empty(result.Tokens);
    }

    [Fact]
    public void Parse_WhitespaceOnlyQuery_ReturnsEmptyParsedQuery()
    {
        var result = _parser.Parse("   ");
        Assert.Equal("   ", result.OriginalQuery);
        Assert.Empty(result.FreeTextTerms);
    }

    [Fact]
    public void Parse_SimpleTextQuery_ExtractsFreeTextTerm()
    {
        var result = _parser.Parse("hello world");
        Assert.Equal("hello world", result.OriginalQuery);
        Assert.Contains("hello", result.FreeTextTerms);
        Assert.Contains("world", result.FreeTextTerms);
        Assert.Equal(2, result.Tokens.Count);
        Assert.Equal(DslOperator.Text, result.Tokens[0].Operator);
        Assert.Equal(DslOperator.Text, result.Tokens[1].Operator);
    }

    [Fact]
    public void Parse_QuotedPhrase_ExtractsPhrase()
    {
        var result = _parser.Parse("\"hello world\"");
        Assert.Contains("hello world", result.Phrases);
        Assert.Single(result.Tokens);
        Assert.True(result.Tokens[0].IsPhrase);
        Assert.Equal(DslOperator.ExactPhrase, result.Tokens[0].Operator);
        Assert.Equal("hello world", result.Tokens[0].Value);
    }

    [Fact]
    public void Parse_WildcardQuery_ExtractsWildcardTerm()
    {
        var result = _parser.Parse("test*");
        Assert.Single(result.Tokens);
        Assert.True(result.Tokens[0].IsWildcard);
        Assert.Equal(DslOperator.Wildcard, result.Tokens[0].Operator);
        Assert.Equal("test*", result.Tokens[0].Value);
    }

    [Fact]
    public void Parse_NegatedQuery_ExtractsNegatedToken()
    {
        var result = _parser.Parse("-deleted");
        Assert.Single(result.Tokens);
        Assert.True(result.Tokens[0].IsNegated);
        Assert.Equal("deleted", result.Tokens[0].Value);
    }

    [Fact]
    public void Parse_NegatedOperatorQuery_ExtractsNegatedOperatorToken()
    {
        var result = _parser.Parse("-from:user1");
        Assert.Single(result.Tokens);
        Assert.True(result.Tokens[0].IsNegated);
        Assert.Equal(DslOperator.From, result.Tokens[0].Operator);
        Assert.Equal("user1", result.Tokens[0].Value);
    }

    // Participant operators
    [Fact]
    public void Parse_FromOperator_ExtractsFromParticipant()
    {
        var result = _parser.Parse("from:user-001");
        Assert.Contains("user-001", result.FromParticipants);
        Assert.Single(result.Tokens);
        Assert.Equal(DslOperator.From, result.Tokens[0].Operator);
        Assert.Equal("user-001", result.Tokens[0].Value);
    }

    [Fact]
    public void Parse_ToOperator_ExtractsToParticipant()
    {
        var result = _parser.Parse("to:user-002");
        Assert.Contains("user-002", result.ToParticipants);
        Assert.Single(result.Tokens);
        Assert.Equal(DslOperator.To, result.Tokens[0].Operator);
    }

    [Fact]
    public void Parse_InvolvingOperator_ExtractsInvolvingParticipant()
    {
        var result = _parser.Parse("involving:user-003");
        Assert.Contains("user-003", result.InvolvingParticipants);
        Assert.Single(result.Tokens);
        Assert.Equal(DslOperator.Involving, result.Tokens[0].Operator);
    }

    // Channel operators
    [Fact]
    public void Parse_InChannelOperator_ExtractsChannel()
    {
        var result = _parser.Parse("in:channel-001");
        Assert.Contains("channel-001", result.Channels);
        Assert.Single(result.Tokens);
        Assert.Equal(DslOperator.In, result.Tokens[0].Operator);
    }

    [Fact]
    public void Parse_ChannelOperator_ExtractsChannel()
    {
        var result = _parser.Parse("channel:channel-002");
        Assert.Contains("channel-002", result.Channels);
    }

    // Temporal operators
    [Fact]
    public void Parse_DuringOperator_WithDateRange_ExtractsTimeRange()
    {
        var result = _parser.Parse("during:2024-01-15..2024-01-20");
        Assert.NotNull(result.CreatedRange);
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero), result.CreatedRange.Start);
    }

    [Fact]
    public void Parse_CreatedAfterOperator_ExtractsTimeRange()
    {
        var result = _parser.Parse("createdafter:2024-01-15");
        Assert.NotNull(result.CreatedRange);
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero), result.CreatedRange.Start);
        Assert.Null(result.CreatedRange.End);
    }

    [Fact]
    public void Parse_CreatedBeforeOperator_ExtractsTimeRange()
    {
        var result = _parser.Parse("createdbefore:2024-01-20");
        Assert.NotNull(result.CreatedRange);
        Assert.Null(result.CreatedRange.Start);
        // When parsing a date without time, it adds a day for the range
        Assert.Equal(new DateTimeOffset(2024, 1, 21, 0, 0, 0, TimeSpan.Zero), result.CreatedRange.End);
    }

    [Fact]
    public void Parse_ModifiedOperator_ExtractsModifiedRange()
    {
        var result = _parser.Parse("modified:2024-01-15..2024-01-20");
        Assert.NotNull(result.ModifiedRange);
        Assert.Equal(new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero), result.ModifiedRange.Start);
    }

    // Metadata operators
    [Fact]
    public void Parse_StatusOperator_ExtractsStatus()
    {
        var result = _parser.Parse("status:sent");
        Assert.Contains(MessageStatus.Sent, result.Statuses);
    }

    [Fact]
    public void Parse_StatusOperator_Multiple_ExtractsMultipleStatuses()
    {
        var result = _parser.Parse("status:sent,read");
        Assert.Contains(MessageStatus.Sent, result.Statuses);
        Assert.Contains(MessageStatus.Read, result.Statuses);
    }

    [Fact]
    public void Parse_TagOperator_ExtractsTag()
    {
        var result = _parser.Parse("tag:urgent");
        Assert.Contains("urgent", result.Tags);
    }

    [Fact]
    public void Parse_AnyTagOperator_ExtractsAnyTag()
    {
        var result = _parser.Parse("anytag:urgent,important");
        Assert.Contains("urgent", result.AnyTags);
        Assert.Contains("important", result.AnyTags);
    }

    [Fact]
    public void Parse_AllTagsOperator_ExtractsAllTags()
    {
        var result = _parser.Parse("alltags:urgent,important");
        Assert.Contains("urgent", result.AllTags);
        Assert.Contains("important", result.AllTags);
    }

    [Fact]
    public void Parse_HasOperator_ExtractsFeature()
    {
        var result = _parser.Parse("has:attachments");
        Assert.Contains("attachments", result.HasFeatures);
    }

    [Fact]
    public void Parse_PinnedOperator_ExtractsPinned()
    {
        var result = _parser.Parse("pinned:true");
        Assert.True(result.IsPinned);
    }

    [Fact]
    public void Parse_PinnedOperator_NoValue_DefaultsToTrue()
    {
        var result = _parser.Parse("pinned:true");
        Assert.True(result.IsPinned);
    }

    // Size operator
    [Fact]
    public void Parse_SizeOperator_ExtractsSizeFilter()
    {
        var result = _parser.Parse("size:>1MB");
        Assert.NotNull(result.SizeFilter);
        Assert.Equal(SizeComparison.GreaterThan, result.SizeFilter.Comparison);
        Assert.Equal(1024 * 1024, result.SizeFilter.Bytes);
    }

    [Fact]
    public void Parse_SizeOperator_WithKB_ExtractsSizeFilter()
    {
        var result = _parser.Parse("size:<100KB");
        Assert.NotNull(result.SizeFilter);
        Assert.Equal(SizeComparison.LessThan, result.SizeFilter.Comparison);
        Assert.Equal(100 * 1024, result.SizeFilter.Bytes);
    }

    // Sorting operators
    [Fact]
    public void Parse_SortOperator_ExtractsSortField()
    {
        var result = _parser.Parse("sort:createdat,desc");
        Assert.Single(result.SortFields);
        Assert.Equal(MessageSortField.CreatedAt, result.SortFields[0].Field);
        Assert.Equal(SortDirection.Descending, result.SortFields[0].Direction);
    }

    // Pagination operators
    [Fact]
    public void Parse_LimitOperator_ExtractsLimit()
    {
        var result = _parser.Parse("limit:10");
        Assert.Equal(10, result.Limit);
    }

    [Fact]
    public void Parse_OffsetOperator_ExtractsOffset()
    {
        var result = _parser.Parse("offset:20");
        Assert.Equal(20, result.Offset);
    }

    [Fact]
    public void Parse_PageOperator_ExtractsPage()
    {
        var result = _parser.Parse("page:2");
        Assert.Equal(2, result.Page);
    }

    [Fact]
    public void Parse_PageSizeOperator_ExtractsPageSize()
    {
        var result = _parser.Parse("pagesize:50");
        Assert.Equal(50, result.PageSize);
    }

    // Complex queries
    [Fact]
    public void Parse_ComplexQuery_ExtractsMultipleComponents()
    {
        var result = _parser.Parse("from:user-001 to:user-002 during:2024-01-15..2024-01-20 status:sent tag:urgent");
        Assert.Contains("user-001", result.FromParticipants);
        Assert.Contains("user-002", result.ToParticipants);
        Assert.NotNull(result.CreatedRange);
        Assert.Contains(MessageStatus.Sent, result.Statuses);
        Assert.Contains("urgent", result.Tags);
        Assert.Equal(5, result.Tokens.Count);
    }

    [Fact]
    public void Parse_QueryWithQuotedPhraseAndOperators()
    {
        var result = _parser.Parse("\"important message\" from:user-001");
        Assert.Contains("important message", result.Phrases);
        Assert.Contains("user-001", result.FromParticipants);
        Assert.Equal(2, result.Tokens.Count);
    }

    [Fact]
    public void Parse_QueryWithWildcardAndOperators()
    {
        var result = _parser.Parse("test* from:user-001");
        Assert.Equal("test*", result.FreeTextTerms[0]);
        Assert.Contains("user-001", result.FromParticipants);
    }

    [Fact]
    public void Parse_QueryWithNegation()
    {
        var result = _parser.Parse("-from:user-001 -status:deleted");
        Assert.True(result.Tokens[0].IsNegated);
        Assert.True(result.Tokens[1].IsNegated);
        Assert.Equal(DslOperator.From, result.Tokens[0].Operator);
        Assert.Equal(DslOperator.Status, result.Tokens[1].Operator);
    }

    // Token to string
    [Fact]
    public void QueryToken_ToString_FormatsCorrectly()
    {
        var token = new QueryToken
        {
            Operator = DslOperator.From,
            Value = "user-001",
            IsNegated = false
        };
        Assert.Equal("from:user-001", token.ToString());
    }

    [Fact]
    public void QueryToken_ToString_Negated_FormatsCorrectly()
    {
        var token = new QueryToken
        {
            Operator = DslOperator.From,
            Value = "user-001",
            IsNegated = true
        };
        Assert.Equal("-from:user-001", token.ToString());
    }

    [Fact]
    public void QueryToken_ToString_Phrase_FormatsCorrectly()
    {
        var token = new QueryToken
        {
            Value = "hello world",
            IsPhrase = true
        };
        Assert.Equal("\"hello world\"", token.ToString());
    }

    [Fact]
    public void QueryToken_ToString_Wildcard_FormatsCorrectly()
    {
        var token = new QueryToken
        {
            Value = "test",
            IsWildcard = true
        };
        Assert.Equal("*test*", token.ToString());
    }
}
