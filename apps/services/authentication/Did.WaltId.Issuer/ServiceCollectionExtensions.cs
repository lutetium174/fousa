using Did.Walt.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Did.WaltId.Issuer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWaltIdIssuer(this IServiceCollection services)
    {
        services
            .AddTransient(provider => new ApiKeyMessageHandler(provider
                .GetRequiredService<IOptions<WaltIdOptions>>().Value
                .IssuerApi
                .ApiKey
            ))
            .AddHttpClient<HttpClient>(
                nameof(WaltIssuerClient),
                (context, client) =>
                {
                    client.BaseAddress = context.GetRequiredService<IOptions<WaltIdOptions>>().Value.IssuerApi.Url;
                })
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            })
#endif
            .AddHttpMessageHandler(provider => provider.GetRequiredService<ApiKeyMessageHandler>());
        return services;
    }
}