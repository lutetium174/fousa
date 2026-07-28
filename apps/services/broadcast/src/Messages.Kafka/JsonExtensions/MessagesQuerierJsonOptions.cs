using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Messages.Kafka.JsonExtensions;

public static class MessagesQuerierJsonOptions
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new CultureInfoJsonConverter() }
    };

    public static IServiceCollection ConfigureJsonOptions(this IServiceCollection services)
    {
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Add(new CultureInfoJsonConverter());
        });
        return services;
    }
}