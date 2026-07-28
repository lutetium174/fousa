using Microsoft.AspNetCore.Builder;

namespace Foundation.DependencyInjection.Web;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddModule<T>(this WebApplicationBuilder builder)
        where T : IModule, new()
    {
        new T().Register(builder.Services, builder.Configuration);
        return builder;
    }
}