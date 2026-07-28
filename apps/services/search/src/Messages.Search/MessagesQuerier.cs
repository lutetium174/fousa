using Core;

namespace Messages.Search;

public class MessagesQuerier : Core.IMessagesQuerier
{
    private readonly List<Core.Message> _messages = new();
    private readonly QueryParser _queryParser = new();

    public MessagesQuerier()
    {
        SeedTestData();
    }

    public MessagesQuerier(List<Message> messages)
    {
        _messages = messages;
    }

    private void SeedTestData()
    {
        var now = DateTimeOffset.UtcNow;
        
        _messages.AddRange(new List<Message>
        {
            new Message
            {
                Id = "msg-001",
                Subject = "Welcome to the team",
                Body = "Hello and welcome to our project!",
                SenderId = "user-001",
                SenderName = "Alice",
                RecipientIds = new List<string> { "user-002" },
                ChannelId = "channel-general",
                ChannelName = "General",
                CreatedAt = now.AddDays(-10),
                ModifiedAt = now.AddDays(-10),
                Status = MessageStatus.Sent,
                Tags = new List<string> { "welcome", "onboarding" },
                IsPinned = false,
                HasAttachments = false,
                SizeInBytes = 512
            },
            new Message
            {
                Id = "msg-002",
                Subject = "Project Update",
                Body = "The project is on track for delivery next week.",
                SenderId = "user-002",
                SenderName = "Bob",
                RecipientIds = new List<string> { "user-001", "user-003" },
                ChannelId = "channel-projects",
                ChannelName = "Projects",
                CreatedAt = now.AddDays(-5),
                ModifiedAt = now.AddDays(-4),
                Status = MessageStatus.Sent,
                Tags = new List<string> { "project", "update" },
                IsPinned = true,
                HasAttachments = true,
                SizeInBytes = 2048
            },
            new Message
            {
                Id = "msg-003",
                Subject = "Urgent: Server Down",
                Body = "The production server is down. Please investigate immediately.",
                SenderId = "user-003",
                SenderName = "Charlie",
                RecipientIds = new List<string> { "user-001", "user-002", "user-004" },
                ChannelId = "channel-alerts",
                ChannelName = "Alerts",
                CreatedAt = now.AddDays(-1),
                ModifiedAt = now.AddHours(-2),
                Status = MessageStatus.Read,
                Tags = new List<string> { "urgent", "server", "alert" },
                IsPinned = true,
                HasAttachments = false,
                SizeInBytes = 1024
            },
            new Message
            {
                Id = "msg-004",
                Subject = "Weekly Meeting Notes",
                Body = "Here are the notes from our weekly meeting.",
                SenderId = "user-001",
                SenderName = "Alice",
                RecipientIds = new List<string> { "user-002", "user-003", "user-004" },
                ChannelId = "channel-meetings",
                ChannelName = "Meetings",
                CreatedAt = now.AddDays(-7),
                ModifiedAt = now.AddDays(-6),
                Status = MessageStatus.Archived,
                Tags = new List<string> { "meeting", "notes" },
                IsPinned = false,
                HasAttachments = true,
                SizeInBytes = 4096
            },
            new Message
            {
                Id = "msg-005",
                Subject = "Draft: New Feature Proposal",
                Body = "Here is a draft proposal for a new feature we should add.",
                SenderId = "user-004",
                SenderName = "Diana",
                RecipientIds = new List<string> { "user-001" },
                ChannelId = "channel-ideas",
                ChannelName = "Ideas",
                CreatedAt = now.AddDays(-3),
                ModifiedAt = now.AddDays(-2),
                Status = MessageStatus.Draft,
                Tags = new List<string> { "feature", "proposal" },
                IsPinned = false,
                HasAttachments = false,
                SizeInBytes = 768
            }
        });
    }

    public Core.IMessageQueryBuilder Query()
    {
        return new MessageQueryBuilder();
    }

    public async Task<IReadOnlyList<Core.Message>> SearchAsync(Core.IMessageQuery query, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        
        if (query is ParsedQuery parsedQuery)
        {
            return SearchWithParsedQuery(parsedQuery);
        }
        
        // Default: return all messages
        return _messages.AsReadOnly();
    }

    public async Task<IReadOnlyList<Message>> FullTextSearchAsync(string searchText, FullTextSearchOptions? options = null,
        CancellationToken ct = default)
    {
        await Task.CompletedTask;
        
        var query = new ParsedQuery
        {
            OriginalQuery = searchText,
            FreeTextTerms = new List<string> { searchText }
        };
        
        return SearchWithParsedQuery(query);
    }

    public async Task<Message?> GetByIdAsync(string messageId, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        return _messages.FirstOrDefault(m => m.Id == messageId);
    }

    public async Task<int> CountAsync(Core.IMessageQuery? query = null, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        
        if (query == null)
            return _messages.Count;
        
        if (query is ParsedQuery parsedQuery)
        {
            var results = SearchWithParsedQuery(parsedQuery);
            return results.Count;
        }
        
        return _messages.Count;
    }

    public async Task<bool> AnyAsync(Core.IMessageQuery? query = null, CancellationToken ct = default)
    {
        await Task.CompletedTask;
        
        if (query == null)
            return _messages.Any();
        
        if (query is ParsedQuery parsedQuery)
        {
            var results = SearchWithParsedQuery(parsedQuery);
            return results.Any();
        }
        
        return _messages.Any();
    }

    private IReadOnlyList<Message> SearchWithParsedQuery(ParsedQuery parsedQuery)
    {
        var query = _messages.AsEnumerable();

        // Apply text search
        if (parsedQuery.FreeTextTerms.Count > 0)
        {
            foreach (var term in parsedQuery.FreeTextTerms)
            {
                query = query.Where(m => 
                    m.Subject.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Body.Contains(term, StringComparison.OrdinalIgnoreCase));
            }
        }

        // Apply phrase search
        if (parsedQuery.Phrases.Count > 0)
        {
            foreach (var phrase in parsedQuery.Phrases)
            {
                query = query.Where(m => 
                    m.Subject.Contains(phrase, StringComparison.OrdinalIgnoreCase) ||
                    m.Body.Contains(phrase, StringComparison.OrdinalIgnoreCase));
            }
        }

        // Apply from filter
        if (parsedQuery.FromParticipants.Count > 0)
        {
            query = query.Where(m => parsedQuery.FromParticipants.Contains(m.SenderId));
        }

        // Apply to filter
        if (parsedQuery.ToParticipants.Count > 0)
        {
            query = query.Where(m => m.RecipientIds.Any(parsedQuery.ToParticipants.Contains));
        }

        // Apply involving filter
        if (parsedQuery.InvolvingParticipants.Count > 0)
        {
            query = query.Where(m => 
                parsedQuery.InvolvingParticipants.Contains(m.SenderId) ||
                m.RecipientIds.Any(parsedQuery.InvolvingParticipants.Contains));
        }

        // Apply channel filter
        if (parsedQuery.Channels.Count > 0)
        {
            query = query.Where(m => parsedQuery.Channels.Contains(m.ChannelId));
        }

        // Apply created range filter
        if (parsedQuery.CreatedRange != null)
        {
            if (parsedQuery.CreatedRange.Start.HasValue)
                query = query.Where(m => m.CreatedAt >= parsedQuery.CreatedRange.Start.Value);
            if (parsedQuery.CreatedRange.End.HasValue)
                query = query.Where(m => m.CreatedAt <= parsedQuery.CreatedRange.End.Value);
        }

        // Apply modified range filter
        if (parsedQuery.ModifiedRange != null)
        {
            if (parsedQuery.ModifiedRange.Start.HasValue)
                query = query.Where(m => m.ModifiedAt >= parsedQuery.ModifiedRange.Start.Value);
            if (parsedQuery.ModifiedRange.End.HasValue)
                query = query.Where(m => m.ModifiedAt <= parsedQuery.ModifiedRange.End.Value);
        }

        // Apply status filter
        if (parsedQuery.Statuses.Count > 0)
        {
            query = query.Where(m => parsedQuery.Statuses.Contains(m.Status));
        }

        // Apply tag filters
        if (parsedQuery.Tags.Count > 0)
        {
            query = query.Where(m => parsedQuery.Tags.Any(t => m.Tags.Contains(t)));
        }

        if (parsedQuery.AnyTags.Count > 0)
        {
            query = query.Where(m => m.Tags.Any(parsedQuery.AnyTags.Contains));
        }

        if (parsedQuery.AllTags.Count > 0)
        {
            query = query.Where(m => parsedQuery.AllTags.All(m.Tags.Contains));
        }

        // Apply has feature filter
        if (parsedQuery.HasFeatures.Count > 0)
        {
            foreach (var feature in parsedQuery.HasFeatures)
            {
                if (feature.Equals("attachments", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(m => m.HasAttachments);
                else if (feature.Equals("links", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(m => m.HasLinks);
            }
        }

        // Apply excluded has feature filter
        if (parsedQuery.ExcludedHasFeatures.Count > 0)
        {
            foreach (var feature in parsedQuery.ExcludedHasFeatures)
            {
                if (feature.Equals("attachments", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(m => !m.HasAttachments);
                else if (feature.Equals("links", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(m => !m.HasLinks);
            }
        }

        // Apply pinned filter
        if (parsedQuery.IsPinned.HasValue)
        {
            query = query.Where(m => m.IsPinned == parsedQuery.IsPinned.Value);
        }

        // Apply size filter
        if (parsedQuery.SizeFilter != null && parsedQuery.SizeFilter.Bytes.HasValue)
        {
            var bytes = parsedQuery.SizeFilter.Bytes.Value;
            switch (parsedQuery.SizeFilter.Comparison)
            {
                case SizeComparison.Equal:
                    query = query.Where(m => m.SizeInBytes == bytes);
                    break;
                case SizeComparison.GreaterThan:
                    query = query.Where(m => m.SizeInBytes > bytes);
                    break;
                case SizeComparison.GreaterThanOrEqual:
                    query = query.Where(m => m.SizeInBytes >= bytes);
                    break;
                case SizeComparison.LessThan:
                    query = query.Where(m => m.SizeInBytes < bytes);
                    break;
                case SizeComparison.LessThanOrEqual:
                    query = query.Where(m => m.SizeInBytes <= bytes);
                    break;
            }
        }

        // Apply sorting
        if (parsedQuery.SortField.HasValue)
        {
            var field = parsedQuery.SortField.Value;
            var direction = parsedQuery.SortDirection;
            
            query = direction == SortDirection.Ascending
                ? query.OrderBy(GetSortKey(field))
                : query.OrderByDescending(GetSortKey(field));
        }
        else
        {
            // Default sorting by CreatedAt descending
            query = query.OrderByDescending(m => m.CreatedAt);
        }

        // Apply pagination
        var offset = parsedQuery.Offset ?? 0;
        var limit = parsedQuery.Limit;
        var page = parsedQuery.Page;
        var pageSize = parsedQuery.PageSize;

        if (page.HasValue && pageSize.HasValue)
        {
            offset = (page.Value - 1) * pageSize.Value;
            limit = pageSize.Value;
        }

        if (offset > 0)
            query = query.Skip(offset);
        if (limit.HasValue)
            query = query.Take(limit.Value);

        return query.ToList().AsReadOnly();
    }

    private Func<Message, object> GetSortKey(MessageSortField field)
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

    public IReadOnlyList<Message> GetAllMessages() => _messages.AsReadOnly();
    public void AddMessage(Message message) => _messages.Add(message);
    public void ClearMessages() => _messages.Clear();
}