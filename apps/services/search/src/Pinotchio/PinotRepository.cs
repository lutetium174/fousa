namespace Pinotchio;

/// <summary>
/// Dataset-based repository for Kafka Pinot
/// Each repository instance represents a dataset/table that can be queried
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public class PinotRepository<TEntity>(
    IPinotClient client,
    string? tableName = null,
    bool supportOrderBy = false)
    : IReadRepository<TEntity>
    where TEntity : class
{
    private readonly IQueryProvider _provider =
        new PinotQueryProvider(client, ResolveTableName(tableName), supportOrderBy);

    public IQueryable<TEntity> Query()
        => new PinotQueryable<TEntity>(_provider);

    private static string ResolveTableName(string? tableName)
    {
        if (!string.IsNullOrEmpty(tableName))
            return tableName;
        
        var resolveTableName = ConfigurationRegistry.GetTableName<TEntity>();
        return !string.IsNullOrEmpty(resolveTableName)
            ? resolveTableName
            : (typeof(TEntity).GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() is TableAttribute
                tableAttribute
                ? tableAttribute.Name
                : null) ?? typeof(TEntity).Name;
    }
}