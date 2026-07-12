using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Messages.Kafka.JsonExtensions;

public static class MessagesQuerierJsonOptions
{
    public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new CultureInfoJsonConverter() }
    };

    public static IServiceCollection ConfigureJsonOptions(this IServiceCollection services)
    {
        return services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new CultureInfoJsonConverter());
        });
    }
}