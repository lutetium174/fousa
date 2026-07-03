namespace Core;

public class MessagesFilter
{
    public string? RoutingKey { get; set; }
    public string? ContentContains { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Limit { get; set; }
    public List<string>? Tags { get; set; }
    public Guid? Sender { get; set; }
    
    // Empty filter means return all messages
    public bool IsEmpty => 
        string.IsNullOrEmpty(RoutingKey) &&
        string.IsNullOrEmpty(ContentContains) &&
        !FromDate.HasValue &&
        !ToDate.HasValue &&
        !Limit.HasValue &&
        (Tags == null || Tags.Count == 0) &&
        !Sender.HasValue;
}
