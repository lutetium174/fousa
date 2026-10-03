namespace Core;

/// <summary>
/// Generic repository interface that supports LINQ queries
/// Can be implemented for any RDBMS (SQL Server, PostgreSQL, MySQL, etc.)
/// or analytical stores (Kafka Pinot, Elasticsearch, etc.)
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IRepository<out TEntity> where TEntity : class
{
    /// <summary>
    /// Gets the queryable collection of entities
    /// This enables LINQ-to-SQL translation when using RDBMS providers
    /// </summary>
    IQueryable<TEntity> Query();
}
