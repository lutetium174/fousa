# Foundation

A foundation library providing core interfaces and types for modular ASP.NET Core applications.

## Features

- `IModule` interface for creating modular application components
- `Result<T>` and `Error<T>` types for functional error handling
- Core abstractions for dependency injection

## Installation

```bash
dotnet add package Foundation
```

## Usage

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
    }
}
```

### Using Result Type

```csharp
using Foundation;

public class MyService
{
    public Result<string> GetData()
    {
        try
        {
            return "success data";
        }
        catch (Exception ex)
        {
            return new Error<string>(ex.Message);
        }
    }
}
```

## License

MIT