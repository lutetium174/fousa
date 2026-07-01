using Did.Walt.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Did.WaltId.Verifier;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWaltIdVerifier(this IServiceCollection services)
    {
        services
            .AddHttpClient<HttpClient>(
                nameof(WaltVerifierClient),
                (context, client) =>
                    client.BaseAddress = context.GetRequiredService<IOptions<WaltIdOptions>>().Value.VerifierApi.Url)
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            })
#endif
            ;
        return services;
    }
}