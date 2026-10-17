using Microsoft.Extensions.DependencyInjection;

namespace Pinotchio;

public static class PinotExtensions
{
    public static IServiceCollection AddPinotClient(
        this IServiceCollection services,
        Action<PinotOptions>? options = null)
    {
        var pinotOptions = new PinotOptions();
        options?.Invoke(pinotOptions);
        services
            .AddSingleton<IPinotClient, PinotClient>()
            .AddHttpClient("Pinot", client => client.BaseAddress = new(pinotOptions.ControllerUri));
        
        return services;
    }
}
