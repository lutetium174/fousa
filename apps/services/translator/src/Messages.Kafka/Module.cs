using Foundation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Messages.Kafka;

public class Module : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
        => services
            .Configure<KafkaConfig>(configuration.GetSection(KafkaConfig.Kafka))
            .AddHostedService<KafkaConsumerWorker>();
}