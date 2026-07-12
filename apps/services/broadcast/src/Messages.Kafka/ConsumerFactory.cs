using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Messages.Kafka;

public class ConsumerFactory(IOptions<KafkaOptions> options)
{
    private readonly KafkaOptions _options = options.Value;

    public IConsumer<byte[], byte[]> CreateConsumer(string groupId)
        => new ConsumerBuilder<byte[], byte[]>(_options is { Username: not null, Password: not null }
                ? SecureConsumer(groupId)
                : BasicConsumer(groupId))
            .Build();

    private ConsumerConfig BasicConsumer(string groupId)
        => new()
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            ClientId = _options.ClientId ?? "message-broker-consumer"
        };

    private ConsumerConfig SecureConsumer(string groupId)
        => new()
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            ClientId = _options.ClientId ?? "message-broker-consumer",
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = _options.Username,
            SaslPassword = _options.Password
        };
}