using Did.Walt.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Did.WaltId.Wallet;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWaltIdWallet(this IServiceCollection services)
    {
        services
            .AddTransient(provider => new ApiKeyMessageHandler(provider
                .GetRequiredService<IOptions<WaltIdOptions>>().Value
                .WalletApi
                .ApiKey
            ))
            .AddHttpClient<HttpClient>(
                "WalletApi",
                (context, client)
                    => client.BaseAddress = context.GetRequiredService<IOptions<WaltIdOptions>>().Value.WalletApi.Url)
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
