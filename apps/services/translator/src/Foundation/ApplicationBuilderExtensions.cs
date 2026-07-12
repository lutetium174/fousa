using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Foundation;

public static class ApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddModule<T>(this WebApplicationBuilder builder)
        where T : IModule, new()
    {
        new T().Register(builder.Services, builder.Configuration);
        return builder;
    }

    public static HostApplicationBuilder AddModule<T>(this HostApplicationBuilder builder)
        where T : IModule, new()
    {
        new T().Register(builder.Services, builder.Configuration);
        return builder;
    }
}