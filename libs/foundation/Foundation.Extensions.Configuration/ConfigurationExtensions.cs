using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation.Extensions.Configuration;

public static class ConfigurationExtensions
{
    public static T AddOpt<T>(
        this IServiceCollection services,
        IConfiguration configuration,
        string section)
        where T : class, new()
    {
        services.AddOptions<T>(section);
        
        return configuration;
    }
}