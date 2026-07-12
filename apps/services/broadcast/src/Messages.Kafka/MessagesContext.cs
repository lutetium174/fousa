using Kafka.LinqToKafka;
using Messages.Kafka.Models;
using Microsoft.Extensions.Options;

namespace Messages.Kafka;

public class MessagesContext(IHttpClientFactory factory, IOptions<PinotOptions> options)
    : KafkaContext(factory, Options.Create(new KafkaQueryOptions
    {
        BaseUrl = options.Value.ControllerUri,
    }))
{
    private readonly KafkaSet<Message>? _messages; 
    public KafkaSet<Message> Messages => _messages ?? Set<Message>();

    private readonly KafkaSet<GermanMessage>? _germanMessages;
    public KafkaSet<GermanMessage> GermanMessages => _germanMessages ?? Set<GermanMessage>();
}