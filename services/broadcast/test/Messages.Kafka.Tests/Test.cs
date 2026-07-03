using Microsoft.Extensions.DependencyInjection;

namespace Messages.Kafka.Tests;

public static class Test
{
    public static async Task Run(
        Func<IServiceProvider, Task> run, 
        Func<IServiceCollection, IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        configure?.Invoke(services);
        await run(services.BuildServiceProvider());
    }
}