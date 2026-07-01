using Foundation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace EventBus.Rabbit;

public class Module : IConfigurableModule
{
    public IServiceCollection Register(IServiceCollection services, IConfiguration configuration)
        => services
            .Configure<RabbitOptions>(_ => configuration.GetSection(nameof(RabbitMQ)))
            .AddTransient<IEventBus>()
            .AddKeyedSingleton<ConnectionFactory>(
                nameof(RabbitMQ),
                (context, _) =>
                {
                    var options = context.GetRequiredService<IOptions<RabbitOptions>>().Value;
                    return new();
                });
}

public record RabbitOptions
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}