using System;
using System.Collections.Concurrent;

namespace Pinotchio;

/// <summary>
/// Registry for entity configurations that allows central configuration management.
/// Similar to Entity Framework's model configuration.
/// </summary>
public static class ConfigurationRegistry
{
    private static readonly ConcurrentDictionary<Type, object> _configurations = new();

    /// <summary>
    /// Registers a configuration for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="configuration">The configuration instance</param>
    public static void Register<TEntity>(EntityConfiguration<TEntity> configuration) where TEntity : class
    {
        _configurations[typeof(TEntity)] = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Gets the configuration for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <returns>The configuration instance, or null if not found</returns>
    public static EntityConfiguration<TEntity>? GetConfiguration<TEntity>() where TEntity : class
    {
        return _configurations.TryGetValue(typeof(TEntity), out var config) 
            ? config as EntityConfiguration<TEntity> 
            : null;
    }

    /// <summary>
    /// Gets the table name for the specified entity type from configuration.
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <returns>The table name if configured, or null</returns>
    public static string? GetTableName<TEntity>() where TEntity : class
    {
        var configuration = GetConfiguration<TEntity>();
        return configuration?.TableName;
    }

    /// <summary>
    /// Clears all registered configurations.
    /// </summary>
    public static void Clear()
    {
        _configurations.Clear();
    }
}