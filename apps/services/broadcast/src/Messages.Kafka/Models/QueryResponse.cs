namespace Messages.Kafka.Models;

public class QueryResponse
{
    public ResultTable? ResultTable { get; set; }
    public int NumRowsResultSet { get; set; }
    public List<object>? Exceptions { get; set; }
    public string? RequestId { get; set; }
    public string? ClientRequestId { get; set; }
    public string? BrokerId { get; set; }
    public List<string>? TablesQueried { get; set; }
    public List<int>? Pools { get; set; }
}