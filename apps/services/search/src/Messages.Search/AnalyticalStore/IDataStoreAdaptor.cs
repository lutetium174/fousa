using Core;
using System.Linq;
using System.Linq.Expressions;

namespace Messages.Search.AnalyticalStore;

/// <summary>
/// Interface for analytical store adapters (like Kafka Pinot, Elasticsearch, etc.)
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IAnalyticalStoreAdaptor<TEntity> where TEntity : class
{
    /// <summary>
    /// The name of the analytical store
    /// </summary>
    string StoreName { get; }

    /// <summary>
    /// The table/index name in the analytical store
    /// </summary>
    string TableName { get; }

    /// <summary>
    /// Translates a LINQ expression to the analytical store's query language
    /// </summary>
    /// <param name="query">The LINQ query to translate</param>
    /// <returns>The translated query in the store's query language</returns>
    string TranslateQuery(IQueryable<TEntity> query);

    /// <summary>
    /// Translates an expression to the analytical store's filter syntax
    /// </summary>
    /// <param name="expression">The expression to translate</param>
    /// <returns>The translated filter string</returns>
    string TranslateExpression(Expression expression);

    /// <summary>
    /// Translates a predicate expression to the analytical store's filter syntax
    /// </summary>
    /// <param name="predicate">The predicate expression</param>
    /// <returns>The translated filter string</returns>
    string TranslatePredicate(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Translates sorting to the analytical store's sort syntax
    /// </summary>
    /// <param name="sortExpressions">The sort expressions</param>
    /// <returns>The translated sort string</returns>
    string TranslateSorting(IEnumerable<(Expression<Func<TEntity, object>> KeySelector, SortDirection Direction)> sortExpressions);

    /// <summary>
    /// Executes a query against the analytical store
    /// </summary>
    /// <param name="query">The translated query string</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of results</returns>
    Task<IReadOnlyList<TEntity>> ExecuteQueryAsync(string query, CancellationToken ct = default);

    /// <summary>
    /// Executes a LINQ query against the analytical store
    /// </summary>
    /// <param name="query">The LINQ query</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of results</returns>
    Task<IReadOnlyList<TEntity>> ExecuteQueryAsync(IQueryable<TEntity> query, CancellationToken ct = default);

    /// <summary>
    /// Gets an entity by ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>The entity or null if not found</returns>
    Task<TEntity?> GetByIdAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Counts entities matching the predicate
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Count of matching entities</returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);

    /// <summary>
    /// Checks if any entity matches the predicate
    /// </summary>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if any entities match</returns>
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);
}

/// <summary>
/// Factory for creating analytical store adapters
/// </summary>
public interface IAnalyticalStoreAdapterFactory
{
    /// <summary>
    /// Creates an adapter for Kafka Pinot
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="tableName">The table name</param>
    /// <param name="connectionString">The connection string or configuration</param>
    /// <returns>Pinot adapter instance</returns>
    IAnalyticalStoreAdaptor<TEntity> CreatePinotAdapter<TEntity>(string tableName, string connectionString) where TEntity : class;

    /// <summary>
    /// Creates an adapter for Elasticsearch
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="indexName">The index name</param>
    /// <param name="connectionString">The connection string or configuration</param>
    /// <returns>Elasticsearch adapter instance</returns>
    IAnalyticalStoreAdaptor<TEntity> CreateElasticsearchAdapter<TEntity>(string indexName, string connectionString) where TEntity : class;

    /// <summary>
    /// Creates a generic analytical store adapter
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="storeType">The type of analytical store</param>
    /// <param name="tableName">The table/index name</param>
    /// <param name="connectionString">The connection string or configuration</param>
    /// <returns>Analytical store adapter instance</returns>
    IAnalyticalStoreAdaptor<TEntity> CreateAdapter<TEntity>(
        string storeType, 
        string tableName, 
        string connectionString) where TEntity : class;
}
