using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Messages.Kafka;

public class ProducerFactory(IOptions<KafkaOptions> options)
{
    private readonly KafkaOptions _options = options.Value;

    public IProducer<string, string> CreateProducer()
        => new ProducerBuilder<string, string>(_options.Username is null && _options.Password is null
                ? BasicProducer()
                : SecureProducer())
            .Build();

    private ProducerConfig SecureProducer()
        => new()
        {
            BootstrapServers = _options.BootstrapServers,
            ClientId = _options.ClientId ?? "message-broker",
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = _options.Username,
            SaslPassword = _options.Password
        };


    private ProducerConfig BasicProducer()
        => new()
        {
            BootstrapServers = _options.BootstrapServers,
            ClientId = _options.ClientId ?? "message-broker"
        };
}