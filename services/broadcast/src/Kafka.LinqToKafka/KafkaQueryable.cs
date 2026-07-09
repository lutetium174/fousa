using System.Collections;
using System.Linq.Expressions;

namespace Kafka.LinqToKafka;

public class KafkaQueryable<T> : IOrderedQueryable<T>
{
    private readonly Expression _expression;
    private readonly KafkaQueryProvider _provider;

    public KafkaQueryable(IEnumerable<T> source)
    {
        _provider = new(source.AsQueryable());
        _expression = Expression.Constant(this);
    }

    internal KafkaQueryable(KafkaQueryProvider provider, Expression expression)
    {
        _provider = provider;
        _expression = expression;
    }

    public IEnumerator<T> GetEnumerator() => _provider.Execute<IEnumerable<T>>(_expression).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public Type ElementType => typeof(T);
    public Expression Expression => _expression;
    public IQueryProvider Provider => _provider;
}