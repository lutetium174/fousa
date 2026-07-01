using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddModule<T>(this IServiceCollection services)
        where T : IModule, new()
    {
        new T().Register(services);
        return services;
    }
    
    public static IServiceCollection AddModule<T>(this IServiceCollection services, IConfiguration configuration)
        where T : IConfigurableModule, new()
    {
        new T().Register(services, configuration);
        return services;
    }
}