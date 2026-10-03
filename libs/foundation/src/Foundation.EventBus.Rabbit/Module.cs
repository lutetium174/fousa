using Foundation.EventBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Foundation.EventBus.Rabbit;

public class Module : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
        => services
            .Configure<RabbitOptions>(_ => configuration.GetSection(nameof(RabbitMQ)))
            .AddTransient<IMessageBroker, EventBus>()
            .AddKeyedSingleton<IConnectionFactory>(
                nameof(RabbitMQ),
                (context, _) =>
                {
                    var options =  context.GetRequiredService<IOptions<RabbitOptions>>().Value;
                    return new ConnectionFactory
                    {
                        HostName = options.Host,
                        UserName = options.Username,
                        Password = options.Password
                    };
                });
}

public record RabbitOptions
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}