using Core;
using Core.Broker;
using Foundation;
using Kafka.LinqToKafka;
using Messages.Kafka.JsonExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Messages.Kafka;

public class Module : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure<KafkaOptions>(configuration.GetSection(KafkaOptions.Kafka));

        services
            .Configure<PinotOptions>(configuration.GetSection(PinotOptions.Pinot))
            .AddHttpClient(PinotOptions.Pinot,
                (provider, client) =>
                {
                    var options = provider.GetRequiredService<IOptions<PinotOptions>>().Value;
                    client.BaseAddress = new(options.ControllerUri);
                });

        services
            .AddSingleton<ProducerFactory>()
            .AddSingleton<IMessageBroker, MessageBroker>()
            .AddSingleton<IMessagesProducer, MessagesProducer>();
            
        services
            .AddTransient<IMessagesQuerier, MessagesQuerier>()
            .ConfigureJsonOptions();
        
        services.AddSingleton<MessagesContext>();
    }
}

public class KafkaOptions
{
    public const string Kafka = "Kafka";
    public string BootstrapServers { get; set; }
    public string Topic { get; set; }
    public string? ConsumerGroupId { get; set; }
    public string? ClientId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class PinotOptions
{
    public const string Pinot = "Pinot";
    public string ControllerUri { get; set; }
    public string? TableName { get; set; }
}