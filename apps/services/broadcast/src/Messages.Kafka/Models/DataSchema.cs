namespace Messages.Kafka.Models;

public class DataSchema
{
    public List<string>? ColumnNames { get; set; }
    public List<string>? ColumnDataTypes { get; set; }
}