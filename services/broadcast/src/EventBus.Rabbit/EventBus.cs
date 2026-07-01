using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.Rabbit;

public class EventBus
{
    private IConnectionFactory? _factory;
    private IChannel? _channel;

    private EventBus(IConnectionFactory? factory)
    {
        _factory = factory;
    }

    public async Task<IChannel> CreateChannel(string host, string user, string pass)
    {
        _factory = new ConnectionFactory
        {
            HostName = host,
            UserName = user,
            Password = pass
        };
        
        var connection = await _factory.CreateConnectionAsync();
        _channel = await connection.CreateChannelAsync();

        await _channel.ExchangeDeclareAsync("event-bus", "topic", durable: true);
        
        return _channel;
    }

    public async Task Publish<T>(string routingKey, T domainEvent, CancellationToken cancellationToken = default)
        where T : DomainEvent
    {
        var json = JsonSerializer.Serialize(domainEvent);
        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync("event-bus", routingKey, body, cancellationToken);
    }

    public async Task Subscribe(string routingKey, Action<string, string> handler)
    {
        var queue = (await _channel.QueueDeclareAsync()).QueueName;
        await _channel.QueueBindAsync(queue, "event-bus", routingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += (model, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            handler(ea.RoutingKey, json);
            return Task.CompletedTask;
        };

        await _channel.BasicConsumeAsync(queue, true, consumer);
    }
}

public class DomainEvent
{
}