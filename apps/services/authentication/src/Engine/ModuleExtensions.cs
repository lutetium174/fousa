using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Engine;

public static class ModuleExtensions
{
    public static IServiceCollection AddModule<T>(this IServiceCollection services, IConfiguration configuration)
        where T : IModule, new()
        => Activator.CreateInstance<T>().Register(services, configuration);
}

public interface IModule
{
    IServiceCollection Register(IServiceCollection services, IConfiguration configuration);
}