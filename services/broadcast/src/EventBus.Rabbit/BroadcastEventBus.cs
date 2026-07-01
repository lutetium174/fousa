using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class BroadcastEventBus
{
    private readonly IModel _channel;

    public BroadcastEventBus(IModel channel)
    {
        _channel = channel;
        _channel.ExchangeDeclare("event-bus", "topic", durable: true);
    }

    public void PublishMessageCreated(object evt)
    {
        Publish("broadcast.message.created", evt);
    }

    private void Publish(string routingKey, object evt)
    {
        var json = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish("event-bus", routingKey, null, body);
    }
}