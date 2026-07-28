using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Messages.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Messages.Kafka;

internal class MessageBroker(
    IOptions<KafkaOptions> options,
    ILogger<MessageBroker> logger) : IMessageBroker, IDisposable
{
    private readonly KafkaOptions _options = options.Value;
    private IProducer<byte[], byte[]>? _producer;
    private readonly Dictionary<string, SubscriptionContext> _subscriptions = new();
    private readonly Lock _lock = new();
    
    private class SubscriptionContext
    {
        public IConsumer<byte[], byte[]> Consumer { get; set; } = null!;
        public CancellationTokenSource Cts { get; set; } = null!;
        public Task? ConsumerTask { get; set; }
    }

    private IProducer<byte[], byte[]> Producer => _producer ??= CreateProducer();
    
    private IProducer<byte[], byte[]> CreateProducer()
    {
        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            ClientId = _options.ClientId ?? "message-broker"
        };
        
        if (!string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
        {
            config.SecurityProtocol = SecurityProtocol.SaslSsl;
            config.SaslMechanism = SaslMechanism.Plain;
            config.SaslUsername = _options.Username;
            config.SaslPassword = _options.Password;
        }
        
        return new ProducerBuilder<byte[], byte[]>(config).Build();
    }

    private IConsumer<byte[], byte[]> CreateConsumer(string groupId)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            ClientId = _options.ClientId ?? "message-broker-consumer"
        };
        
        if (!string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
        {
            config.SecurityProtocol = SecurityProtocol.SaslSsl;
            config.SaslMechanism = SaslMechanism.Plain;
            config.SaslUsername = _options.Username;
            config.SaslPassword = _options.Password;
        }
        
        return new ConsumerBuilder<byte[], byte[]>(config).Build();
    }

    public async Task Publish<T>(string routingKey, T domainEvent, CancellationToken cancellationToken = default)
        where T : DomainEvent
    {
        try
        {
            var json = JsonSerializer.Serialize(domainEvent);
            var body = Encoding.UTF8.GetBytes(json);
            var key = Encoding.UTF8.GetBytes(routingKey);

            var topic = _options.Topic;
            
            await Producer.ProduceAsync(
                topic, 
                new()
                {
                    Key = key,
                    Value = body,
                    Headers = new() { { "routing-key", Encoding.UTF8.GetBytes(routingKey) } }
                },
                cancellationToken);

            logger.LogInformation("Published message to topic {Topic} with key {Key}", topic, routingKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish message with routing key {RoutingKey}", routingKey);
            throw;
        }
    }

    public Task Subscribe(string routingKey, Action<string, string> handler)
    {
        var groupId = $"{_options.ConsumerGroupId ?? "message-broker-group"}-{routingKey}";
        var topic = _options.Topic;
        
        lock (_lock)
        {
            // Check if we're already subscribed to this routing key
            if (_subscriptions.ContainsKey(routingKey))
            {
                logger.LogWarning("Already subscribed to routing key {RoutingKey}", routingKey);
                return Task.CompletedTask;
            }

            var consumer = CreateConsumer(groupId);
            var cts = new CancellationTokenSource();
            
            var context = new SubscriptionContext
            {
                Consumer = consumer,
                Cts = cts
            };
            
            _subscriptions[routingKey] = context;
            
            consumer.Subscribe(topic);
            
            context.ConsumerTask = Task.Run(() => ConsumeMessages(consumer, routingKey, handler, cts.Token), cts.Token);
            
            logger.LogInformation("Subscribed to topic {Topic} with routing key {RoutingKey}", topic, routingKey);
        }
        
        return Task.CompletedTask;
    }

    private async Task ConsumeMessages(IConsumer<byte[], byte[]> consumer, string routingKey, Action<string, string> handler, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(cancellationToken);
                    
                    // Check if this message matches our routing key
                    var messageRoutingKey = routingKey;
                    bool shouldProcess;
                    
                    // Check headers first
                    if (consumeResult.Message.Headers.TryGetLastBytes("routing-key", out var headerValue))
                    {
                        messageRoutingKey = Encoding.UTF8.GetString(headerValue);
                        shouldProcess = messageRoutingKey == routingKey;
                    }
                    else
                    {
                        // If no routing key header, use the message key
                        messageRoutingKey = Encoding.UTF8.GetString(consumeResult.Message.Key);
                        shouldProcess = messageRoutingKey == routingKey;
                    }
                    
                    if (shouldProcess)
                    {
                        var json = Encoding.UTF8.GetString(consumeResult.Message.Value);
                        handler(routingKey, json);
                    }
                }
                catch (ConsumeException e)
                {
                    logger.LogError(e, "Error consuming message for routing key {RoutingKey}", routingKey);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when unsubscribing
            logger.LogInformation("Subscription cancelled for routing key {RoutingKey}", routingKey);
        }
        finally
        {
            lock (_lock)
            {
                if (_subscriptions.TryGetValue(routingKey, out var context) && context.Consumer == consumer)
                {
                    consumer.Close();
                    _subscriptions.Remove(routingKey);
                }
            }
        }
    }

    public void Unsubscribe(string routingKey)
    {
        lock (_lock)
        {
            if (_subscriptions.TryGetValue(routingKey, out var context))
            {
                context.Cts.Cancel();
                context.Consumer.Close();
                _subscriptions.Remove(routingKey);
                context.Cts.Dispose();
                
                logger.LogInformation("Unsubscribed from routing key {RoutingKey}", routingKey);
            }
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
        
        lock (_lock)
        {
            foreach (var context in _subscriptions.Values)
            {
                context.Cts.Cancel();
                try
                {
                    context.ConsumerTask?.Wait(TimeSpan.FromSeconds(5));
                }
                catch
                {
                    // Ignore exceptions during shutdown
                }
                context.Consumer.Close();
                context.Cts.Dispose();
            }
            _subscriptions.Clear();
        }
    }
}