namespace Messages.Kafka;

public class MessageModel
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public Guid Sender { get; set; }
    public string RoutingKey { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}