using Pinotchio;

namespace Messages.Search;

/// <summary>
/// Dataset-based repository for Kafka Pinot
/// Each repository instance represents a dataset/table that can be queried
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public class PinotRepository<TEntity>(IPinotClient client) : IReadRepository<TEntity> where TEntity : class
{
    private readonly IQueryProvider _provider = new PinotQueryProvider(client);
    
    /// <summary>
    /// Gets the queryable dataset
    /// This returns a queryable that will be translated to Pinot SQL when executed
    /// </summary>
    public IQueryable<TEntity> Query()
        => new PinotQueryable<TEntity>(_provider);
}