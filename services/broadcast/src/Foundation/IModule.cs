using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation;

public interface IModule
{
    IServiceCollection Register(IServiceCollection services);
}

public interface IConfigurableModule
{
    IServiceCollection Register(IServiceCollection services, IConfiguration configuration);
}