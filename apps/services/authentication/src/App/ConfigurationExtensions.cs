using Did.Walt.Core;

namespace fousa;

public static class ConfigurationExtensions
{
    public static IHostApplicationBuilder AddWaltIdOptions(this IHostApplicationBuilder host)
    {
        host.Services.Configure<WaltIdOptions>(host.Configuration.GetSection("Walt"));
        return host;
    }
}