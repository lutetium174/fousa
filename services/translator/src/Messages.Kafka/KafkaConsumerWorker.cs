using System.Globalization;
using System.Text.Json.Nodes;
using Confluent.Kafka;
using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Messages.Kafka;

public class KafkaConsumerWorker(
    ITranslator translator,
    IOptions<KafkaConfig> kafkaConfig,
    IConfiguration configuration,
    ILogger<KafkaConsumerWorker> logger)
    : BackgroundService
{
    private readonly KafkaConfig _kafkaConfig = kafkaConfig.Value;
    private IConsumer<string, string>? _consumer;
    private IProducer<string, string>? _producer;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaConfig.BootstrapServers,
            GroupId = _kafkaConfig.Consumer?.GroupId ?? "translator-service",
            AutoOffsetReset = _kafkaConfig.Consumer?.GetAutoOffsetReset() ?? AutoOffsetReset.Earliest,
            EnableAutoCommit = _kafkaConfig.Consumer?.EnableAutoCommit ?? true
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _producer = new ProducerBuilder<string, string>(config).Build();

        foreach (var topic in _kafkaConfig.Consumer?.Topics ?? ["messages"])
        {
            _consumer.Subscribe(topic);
            logger.LogInformation("Subscribed to topic: {Topic}", topic);
        }

        logger.LogInformation("Kafka consumer started. Waiting for messages...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);
                    var message = consumeResult.Message.Value;

                    if (string.IsNullOrEmpty(message)) continue;
                    
                    try
                    {
                        var culture = new CultureInfo(configuration.GetValue<string>("Culture")!);
                        var node = JsonNode.Parse(message);
                        var content = node!["Content"]!.GetValue<string>();
                        
                        var result = await translator.Translate(
                            content,
                            culture,
                            stoppingToken);

                        await result.Match(async success =>
                        {

                            node["Content"] = result.Value;

                            await _producer.ProduceAsync(
                                $"messages-{culture.Name}",
                                new()
                                {
                                    Key = consumeResult.Message.Key,
                                    Value = node.ToJsonString()
                                }, stoppingToken);
                            
                            _consumer.Commit(consumeResult);
                        },
                        failure =>
                        {
                            logger.LogError("Translation failed for message: {Message}. Error: {Error}",
                                content, failure);
                            return Task.CompletedTask;
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing message: {Message}", message);
                    }
                }
                catch (ConsumeException e)
                {
                    logger.LogError(e, "Error consuming message");
                }
                catch (OperationCanceledException) // Expected when stopping
                {
                    break;
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Unexpected error in Kafka consumer");
                }
            }
        }
        finally
        {
            _consumer?.Close();
            logger.LogInformation("Kafka consumer closed");
        }
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        _producer?.Dispose();
        base.Dispose();
    }
}