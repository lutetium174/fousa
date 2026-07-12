namespace Messages.Kafka.Models;

public class ResultTable
{
    public DataSchema? DataSchema { get; set; }
    public List<List<object>>? Rows { get; set; }
}