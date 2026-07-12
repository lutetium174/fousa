using Engine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Did.WaltId.Wallet;

public class WaltIdModule : IModule
{
    public IServiceCollection Register(IServiceCollection services, IConfiguration configuration)
        => services
            .AddTransient<IAuthentication, Authentication>()
            .AddSingleton<ILoginState, LoginState>();
}