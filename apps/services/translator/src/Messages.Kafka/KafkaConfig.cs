namespace Messages.Kafka;

public class KafkaConfig
{
    public const string Kafka = "Kafka";
    
    public string BootstrapServers { get; set; } = "localhost:9092";
    
    public KafkaConsumerConfig? Consumer { get; set; }
}

public class KafkaConsumerConfig
{
    public string GroupId { get; set; } = "translator-service";
    public string AutoOffsetReset { get; set; } = "Earliest";
    public bool EnableAutoCommit { get; set; } = true;
    public List<string> Topics { get; set; } = new();
    
    public Confluent.Kafka.AutoOffsetReset GetAutoOffsetReset()
    {
        return AutoOffsetReset switch
        {
            "Earliest" => Confluent.Kafka.AutoOffsetReset.Earliest,
            "Latest" => Confluent.Kafka.AutoOffsetReset.Latest,
            _ => Confluent.Kafka.AutoOffsetReset.Earliest
        };
    }
}