namespace Messages.Kafka;

internal class KafkaOptions
{
    public const string Kafka = "Kafka";
    public string BootstrapServers { get; set; }
    public string Topic { get; set; }
    public string? ConsumerGroupId { get; set; }
    public string? ClientId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}