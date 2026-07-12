using Did.Walt.Core;

namespace fousa;

public static class ConfigurationExtensions
{
    public static IHostApplicationBuilder AddWaltIdOptions(this IHostApplicationBuilder host)
    {
        var options = new WaltIdOptions();
        
        host.Configuration.Bind("Walt", options);
        host.Services.AddOptions<WaltIdOptions>();
        
        return host;
    }
}