namespace Core;

/// <summary>
/// The Message entity that will be returned by queries
/// </summary>
public class Message
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public List<string> RecipientIds { get; set; } = new();
    public List<string> CcIds { get; set; } = new();
    public List<string> BccIds { get; set; } = new();
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public MessageStatus Status { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool HasAttachments { get; set; }
    public bool HasLinks { get; set; }
    public bool IsPinned { get; set; }
    public long SizeInBytes { get; set; }
    public int ReactionCount { get; set; }
    public List<string> AttachmentTypes { get; set; } = new();
    public Dictionary<string, string> Metadata { get; set; } = new();
}