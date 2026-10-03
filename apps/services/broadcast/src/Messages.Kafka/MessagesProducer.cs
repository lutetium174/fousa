using System.Text.Json;
using Core;
using Core.Broker;

namespace Messages.Kafka;

public class MessagesProducer(ProducerFactory factory) : IMessagesProducer
{
    public async Task Write(Message message, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();

        using var producer = factory.CreateProducer();
        await producer.ProduceAsync(
            message.Topic ?? "messages",
            new()
            {
                Key = id.ToString(),
                Value = JsonSerializer.Serialize(new
                {
                    Id = id,
                    Content = message.Content,
                    Sender = message.Sender,
                    Timestamp = DateTime.UtcNow,
                    RoutingKey = "default"
                })
            },
            cancellationToken);
    }
}