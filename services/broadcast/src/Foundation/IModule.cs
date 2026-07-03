using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foundation;

public interface IModule
{
    void Register(IServiceCollection services, IConfiguration configuration);
}