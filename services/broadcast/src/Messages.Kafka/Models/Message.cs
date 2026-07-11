using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace Messages.Kafka.Models;

[Table("messages")]
public class Message
{
    public string? RoutingKey { get; set; }
    public string? ContentContains { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? Limit { get; set; }
    public List<string>? Tags { get; set; }
    public Guid? Sender { get; set; }
    public CultureInfo? Culture { get; set; }
}