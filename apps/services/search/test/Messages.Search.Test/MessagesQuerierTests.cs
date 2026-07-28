using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core;
using Messages.Search;
using Xunit;

namespace Messages.Search.Test;

public class MessagesQuerierTests : IDisposable
{
    private readonly MessagesQuerier _querier;
    private readonly DateTimeOffset _now;

    public MessagesQuerierTests()
    {
        _now = DateTimeOffset.UtcNow;
        
        var testMessages = new List<Message>
        {
            new Message
            {
                Id = "test-001",
                Subject = "Welcome to the team",
                Body = "Hello and welcome to our project!",
                SenderId = "user-001",
                SenderName = "Alice",
                RecipientIds = new List<string> { "user-002" },
                ChannelId = "channel-general",
                ChannelName = "General",
                CreatedAt = _now.AddDays(-10),
                ModifiedAt = _now.AddDays(-10),
                Status = MessageStatus.Sent,
                Tags = new List<string> { "welcome", "onboarding" },
                IsPinned = false,
                HasAttachments = false,
                SizeInBytes = 512
            },
            new Message
            {
                Id = "test-002",
                Subject = "Project Update",
                Body = "The project is on track for delivery next week.",
                SenderId = "user-002",
                SenderName = "Bob",
                RecipientIds = new List<string> { "user-001", "user-003" },
                ChannelId = "channel-projects",
                ChannelName = "Projects",
                CreatedAt = _now.AddDays(-5),
                ModifiedAt = _now.AddDays(-4),
                Status = MessageStatus.Sent,
                Tags = new List<string> { "project", "update" },
                IsPinned = true,
                HasAttachments = true,
                SizeInBytes = 2048
            },
            new Message
            {
                Id = "test-003",
                Subject = "Urgent: Server Down",
                Body = "The production server is down. Please investigate immediately.",
                SenderId = "user-003",
                SenderName = "Charlie",
                RecipientIds = new List<string> { "user-001", "user-002", "user-004" },
                ChannelId = "channel-alerts",
                ChannelName = "Alerts",
                CreatedAt = _now.AddDays(-1),
                ModifiedAt = _now.AddHours(-2),
                Status = MessageStatus.Read,
                Tags = new List<string> { "urgent", "server", "alert" },
                IsPinned = true,
                HasAttachments = false,
                SizeInBytes = 1024
            },
            new Message
            {
                Id = "test-004",
                Subject = "Weekly Meeting Notes",
                Body = "Here are the notes from our weekly meeting.",
                SenderId = "user-001",
                SenderName = "Alice",
                RecipientIds = new List<string> { "user-002", "user-003", "user-004" },
                ChannelId = "channel-meetings",
                ChannelName = "Meetings",
                CreatedAt = _now.AddDays(-7),
                ModifiedAt = _now.AddDays(-6),
                Status = MessageStatus.Archived,
                Tags = new List<string> { "meeting", "notes" },
                IsPinned = false,
                HasAttachments = true,
                SizeInBytes = 4096
            },
            new Message
            {
                Id = "test-005",
                Subject = "Draft: New Feature Proposal",
                Body = "Here is a draft proposal for a new feature we should add.",
                SenderId = "user-004",
                SenderName = "Diana",
                RecipientIds = new List<string> { "user-001" },
                ChannelId = "channel-ideas",
                ChannelName = "Ideas",
                CreatedAt = _now.AddDays(-3),
                ModifiedAt = _now.AddDays(-2),
                Status = MessageStatus.Draft,
                Tags = new List<string> { "feature", "proposal" },
                IsPinned = false,
                HasAttachments = false,
                SizeInBytes = 768
            }
        };
        
        _querier = new MessagesQuerier(testMessages);
    }

    public void Dispose()
    {
        _querier.ClearMessages();
    }

    [Fact]
    public async Task SearchAsync_WithFromFilter_ReturnsFilteredResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("from:user-001");
        
        var results = await _querier.SearchAsync(query);
        
        Assert.Equal(2, results.Count);
        Assert.All(results, m => Assert.Equal("user-001", m.SenderId));
    }

    [Fact]
    public async Task SearchAsync_WithToFilter_ReturnsFilteredResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("to:user-001");
        
        var results = await _querier.SearchAsync(query);
        
        // user-001 is recipient in test-002, test-003, test-005 = 3 messages
        Assert.Equal(3, results.Count);
        Assert.All(results, m => Assert.Contains("user-001", m.RecipientIds));
    }

    [Fact]
    public async Task SearchAsync_WithInvolvingFilter_ReturnsFilteredResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("involving:user-001");
        
        var results = await _querier.SearchAsync(query);
        
        // user-001 is sender in test-001 and test-004, recipient in test-002, test-003, test-005 = 5 messages
        Assert.Equal(5, results.Count);
    }

    [Fact]
    public async Task SearchAsync_WithChannelFilter_ReturnsFilteredResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("in:channel-projects");
        
        var results = await _querier.SearchAsync(query);
        
        Assert.Single(results);
        Assert.Equal("channel-projects", results[0].ChannelId);
    }

    [Fact]
    public async Task SearchAsync_WithStatusFilter_ReturnsFilteredResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("status:sent");
        
        var results = await _querier.SearchAsync(query);
        
        Assert.Equal(2, results.Count);
        Assert.All(results, m => Assert.Equal(MessageStatus.Sent, m.Status));
    }

    [Fact]
    public async Task SearchAsync_WithTagFilter_ReturnsFilteredResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("tag:urgent");
        
        var results = await _querier.SearchAsync(query);
        
        Assert.Single(results);
        Assert.Equal("test-003", results[0].Id);
    }

    [Fact]
    public async Task SearchAsync_WithPinnedFilter_ReturnsFilteredResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("pinned:true");
        
        var results = await _querier.SearchAsync(query);
        
        Assert.Equal(2, results.Count);
        Assert.All(results, m => Assert.True(m.IsPinned));
    }

    [Fact]
    public async Task SearchAsync_WithTextSearch_ReturnsFilteredResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("server");
        
        var results = await _querier.SearchAsync(query);
        
        Assert.Single(results);
        Assert.Equal("test-003", results[0].Id);
    }

    [Fact]
    public async Task SearchAsync_WithPhraseSearch_ReturnsFilteredResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("\"production server\"");
        
        var results = await _querier.SearchAsync(query);
        
        Assert.Single(results);
        Assert.Equal("test-003", results[0].Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsMessage()
    {
        var message = await _querier.GetByIdAsync("test-001");
        
        Assert.NotNull(message);
        Assert.Equal("test-001", message.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ReturnsNull()
    {
        var message = await _querier.GetByIdAsync("non-existent");
        
        Assert.Null(message);
    }

    [Fact]
    public async Task CountAsync_WithNoQuery_ReturnsTotalCount()
    {
        var count = await _querier.CountAsync();
        
        Assert.Equal(5, count);
    }

    [Fact]
    public async Task CountAsync_WithQuery_ReturnsFilteredCount()
    {
        var parser = new QueryParser();
        var query = parser.Parse("status:sent");
        
        var count = await _querier.CountAsync(query);
        
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task AnyAsync_WithNoQuery_ReturnsTrue()
    {
        var any = await _querier.AnyAsync();
        
        Assert.True(any);
    }

    [Fact]
    public async Task AnyAsync_WithEmptyQuery_ReturnsFalse()
    {
        var parser = new QueryParser();
        var query = parser.Parse("from:non-existent");
        
        var any = await _querier.AnyAsync(query);
        
        Assert.False(any);
    }

    [Fact]
    public async Task FullTextSearchAsync_WithSearchText_ReturnsResults()
    {
        var results = await _querier.FullTextSearchAsync("welcome");
        
        Assert.Single(results);
        Assert.Equal("test-001", results[0].Id);
    }

    [Fact]
    public void Query_ReturnsQueryBuilder()
    {
        var builder = _querier.Query();
        
        Assert.IsType<MessageQueryBuilder>(builder);
    }

    [Fact]
    public async Task SearchAsync_WithQueryBuilder_ReturnsResults()
    {
        var builder = _querier.Query();
        var query = builder
            .FromParticipant("user-001")
            .WithStatus(MessageStatus.Sent)
            .Build();
        
        var results = await _querier.SearchAsync(query);
        
        Assert.Single(results);
        Assert.Equal("test-001", results[0].Id);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsPaginatedResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("");
        query.Page = 1;
        query.PageSize = 2;
        
        var results = await _querier.SearchAsync(query);
        
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task SearchAsync_WithSorting_ReturnsSortedResults()
    {
        var parser = new QueryParser();
        var query = parser.Parse("");
        query.SortField = MessageSortField.CreatedAt;
        query.SortDirection = SortDirection.Ascending;
        
        var results = await _querier.SearchAsync(query);
        
        Assert.Equal(5, results.Count);
        Assert.Equal("test-001", results[0].Id);
        // Sorted by CreatedAt ascending: test-001 (-10), test-004 (-7), test-002 (-5), test-005 (-3), test-003 (-1)
        Assert.Equal("test-003", results[4].Id);
    }
}
