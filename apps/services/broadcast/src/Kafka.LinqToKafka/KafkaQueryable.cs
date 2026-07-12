using System.Collections;
using System.Linq.Expressions;

namespace Kafka.LinqToKafka;

public class KafkaQueryable<T> : IOrderedQueryable<T>
{
    private readonly Expression _expression;
    private readonly IQueryProvider _provider;

    public KafkaQueryable(HttpClient httpClient, string pinotBaseUrl)
    {
        _provider = new KafkaQueryProvider(httpClient, pinotBaseUrl);
        _expression = Expression.Constant(this);
    }

    internal KafkaQueryable(IQueryProvider provider, Expression expression)
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