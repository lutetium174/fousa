# Foundation.DependencyInjection.Web

Foundation library providing ASP.NET Core Web-specific dependency injection extensions for modular applications.

## Features

- `WebApplicationBuilderExtensions` with `AddModule<T>()` extension method
- Seamless integration with ASP.NET Core's dependency injection system
- Supports modular application architecture

## Installation

```bash
dotnet add package Foundation.DependencyInjection.Web
```

## Usage

### Adding Modules to WebApplicationBuilder

```csharp
using Foundation.DependencyInjection.Web;

var builder = WebApplication.CreateBuilder(args);

// Add modules to your application
builder.AddModule<MyModule>();
// Or add multiple modules
builder.AddModule<DatabaseModule>();
builder.AddModule<AuthenticationModule>();

var app = builder.Build();
```

### Creating a Module

```csharp
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

public class MyModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMyService, MyService>();
        services.Configure<MyOptions>(configuration.GetSection("MyOptions"));
    }
}
```

## Dependencies

- Foundation (core package)
- Microsoft.AspNetCore.Builder
- Microsoft.Extensions.DependencyInjection.Abstractions

## License

MIT