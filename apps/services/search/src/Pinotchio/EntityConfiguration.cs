namespace Pinotchio;

/// <summary>
/// Base class for entity configuration that allows fluent mapping configuration.
/// Similar to Entity Framework's entity configuration.
/// </summary>
/// <typeparam name="TEntity">The entity type to configure</typeparam>
public abstract class EntityConfiguration<TEntity> where TEntity : class
{
    /// <summary>
    /// Gets or sets the table name for this entity.
    /// </summary>
    public string? TableName { get; protected set; }

    /// <summary>
    /// Configures the table name for this entity.
    /// </summary>
    /// <param name="tableName">The name of the table</param>
    /// <returns>The configuration instance for method chaining</returns>
    public EntityConfiguration<TEntity> ToTable(string tableName)
    {
        TableName = tableName;
        return this;
    }
}