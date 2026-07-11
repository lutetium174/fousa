using System.Collections;
using System.Linq.Expressions;

namespace Kafka.LinqToKafka;

public class KafkaSet<TEntity> : IOrderedQueryable<TEntity> where TEntity : class
{
    private readonly IQueryable<TEntity> _queryable;

    internal KafkaSet(IQueryable<TEntity> queryable)
    {
        _queryable = queryable ?? throw new ArgumentNullException(nameof(queryable));
    }

    // IQueryable implementation
    public Type ElementType => _queryable.ElementType;
    public Expression Expression => _queryable.Expression;
    public IQueryProvider Provider => _queryable.Provider;

    // Query methods that return KafkaSet for fluent chaining
    public KafkaSet<TEntity> Where(Expression<Func<TEntity, bool>> predicate) => new(_queryable.Where(predicate));

    public KafkaSet<TEntity> OrderBy<TKey>(Expression<Func<TEntity, TKey>> keySelector) =>
        new(_queryable.OrderBy(keySelector));

    public KafkaSet<TEntity> OrderByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector) =>
        new(_queryable.OrderByDescending(keySelector));

    public KafkaSet<TEntity> ThenBy<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        => new(_queryable is IOrderedQueryable<TEntity> ordered 
            ? ordered.ThenBy(keySelector) 
            : _queryable.OrderBy(keySelector));

    public KafkaSet<TEntity> ThenByDescending<TKey>(Expression<Func<TEntity, TKey>> keySelector)
        => new(_queryable is IOrderedQueryable<TEntity> ordered 
            ? ordered.ThenByDescending(keySelector) 
            : _queryable.OrderByDescending(keySelector));

    public KafkaSet<TEntity> Take(int count) => new(_queryable.Take(count));
    public KafkaSet<TEntity> Skip(int count) => new(_queryable.Skip(count));

    // Execution methods
    public List<TEntity> ToList() => _queryable.ToList();
    public TEntity First() => _queryable.First();
    public TEntity? FirstOrDefault() => _queryable.FirstOrDefault();
    public TEntity Single() => _queryable.Single();
    public TEntity? SingleOrDefault() => _queryable.SingleOrDefault();
    public int Count() => _queryable.Count();
    public bool Any() => _queryable.Any();
    public bool Any(Expression<Func<TEntity, bool>> predicate) => _queryable.Any(predicate);

    // IQueryable explicit interface implementation
    IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator() => _queryable.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _queryable.GetEnumerator();
}
