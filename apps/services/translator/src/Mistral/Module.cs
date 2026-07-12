using Core;
using Foundation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Mistral;

public class Module : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services
            .Configure<MistralOptions>(configuration.GetSection(MistralOptions.Mistral))
            .AddHttpClient(nameof(MistralClient), (provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<MistralOptions>>().Value;
                client.BaseAddress = new(options.BaseUri);
                client.DefaultRequestHeaders.Accept.Add(new("application/json"));
            });
        
        services.AddTransient<ITranslator, MistralClient>();
    }
}